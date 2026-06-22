import { chromium } from '@playwright/test';
import { writeFileSync, mkdirSync } from 'fs';

const BASE = 'http://localhost:3000';
const SCREENSHOTS = 'c:/Users/dell/DentalERP/audit_screenshots';
mkdirSync(SCREENSHOTS, { recursive: true });

const PAGES = [
  { name: '01_login', url: '/login', auth: false },
  { name: '02_dashboard', url: '/', auth: true },
  { name: '03_patients', url: '/patients', auth: true },
  { name: '04_patients_new', url: '/patients/new', auth: true },
  { name: '05_appointments', url: '/appointments', auth: true },
  { name: '06_queue', url: '/queue', auth: true },
  { name: '07_settings_services', url: '/settings/services', auth: true },
  { name: '08_settings_vaults', url: '/settings/vaults', auth: true },
  { name: '09_settings_insurance', url: '/settings/insurance', auth: true },
  { name: '10_finance_invoices', url: '/finance/invoices', auth: true },
  { name: '11_finance_installments', url: '/finance/installments', auth: true },
  { name: '12_finance_treasury', url: '/finance/treasury', auth: true },
  { name: '13_finance_insurance_claims', url: '/finance/insurance/claims', auth: true },
  { name: '14_finance_insurance_receivables', url: '/finance/insurance/receivables', auth: true },
];

const ISSUES = [];

async function login(page) {
  await page.goto(`${BASE}/login`);
  await page.waitForSelector('input[name="username"], input[type="text"]', { timeout: 10000 });
  const userInput = page.locator('input[name="username"], input[type="text"]').first();
  const passInput = page.locator('input[type="password"]').first();
  await userInput.fill('admin');
  await passInput.fill('Admin@123');
  await page.locator('button[type="submit"]').click();
  await page.waitForURL(url => !url.includes('/login'), { timeout: 10000 }).catch(() => {});
}

async function checkPage(page, name, url) {
  const fullUrl = `${BASE}${url}`;
  const issues = [];

  try {
    await page.goto(fullUrl, { waitUntil: 'networkidle', timeout: 15000 });
  } catch (e) {
    await page.goto(fullUrl, { waitUntil: 'domcontentloaded', timeout: 15000 });
    await page.waitForTimeout(2000);
  }

  // Check for JS errors
  const consoleErrors = [];
  page.on('console', msg => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });

  await page.waitForTimeout(2000);

  // Check for error text on page
  const bodyText = await page.locator('body').innerText().catch(() => '');

  if (bodyText.includes('404') || bodyText.includes('not found')) {
    issues.push('404 - Page not found');
  }
  if (bodyText.includes('500') || bodyText.includes('Internal Server Error')) {
    issues.push('500 - Server error');
  }
  if (bodyText.includes('Error') && !bodyText.includes('No errors')) {
    // Look for specific error patterns
    const errorMatches = bodyText.match(/Error[:\s][^\n]{0,100}/g);
    if (errorMatches) issues.push(...errorMatches.slice(0, 3).map(e => `UI Error: ${e.trim()}`));
  }

  // Check for failed API calls (network errors)
  const failedRequests = [];
  page.on('requestfailed', req => {
    failedRequests.push(`${req.method()} ${req.url()}`);
  });

  // Screenshot
  await page.screenshot({ path: `${SCREENSHOTS}/${name}.png`, fullPage: true });

  return { name, url, issues, bodyText: bodyText.slice(0, 500) };
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
  const page = await context.newPage();

  // Collect all console errors
  const allErrors = {};
  page.on('console', msg => {
    if (msg.type() === 'error') {
      const current = allErrors[page.url()] || [];
      current.push(msg.text());
      allErrors[page.url()] = current;
    }
  });

  // Collect failed network requests
  const failedReqs = {};
  page.on('requestfailed', req => {
    const url = page.url();
    const current = failedReqs[url] || [];
    current.push(`${req.method()} ${req.url()} — ${req.failure()?.errorText}`);
    failedReqs[url] = current;
  });

  // Login first
  console.log('Logging in...');
  await login(page);
  console.log('Logged in, current URL:', page.url());
  await page.screenshot({ path: `${SCREENSHOTS}/00_after_login.png`, fullPage: true });

  const results = [];

  for (const p of PAGES) {
    if (!p.auth) continue;
    console.log(`Checking: ${p.name} → ${p.url}`);
    try {
      await page.goto(`${BASE}${p.url}`, { waitUntil: 'domcontentloaded', timeout: 15000 });
      await page.waitForTimeout(3000);
      await page.screenshot({ path: `${SCREENSHOTS}/${p.name}.png`, fullPage: true });

      const bodyText = await page.locator('body').innerText().catch(() => '');
      const pageIssues = [];

      if (page.url().includes('/login')) {
        pageIssues.push('REDIRECTED TO LOGIN — not authenticated');
      }
      if (bodyText.match(/error|خطأ/i) && !bodyText.match(/no error/i)) {
        const matches = bodyText.match(/.*(error|خطأ).{0,80}/gi);
        if (matches) pageIssues.push(...matches.slice(0, 2));
      }
      if (bodyText.includes('Cannot read') || bodyText.includes('undefined')) {
        pageIssues.push('JS runtime error visible on page');
      }

      results.push({ page: p.name, url: p.url, finalUrl: page.url(), issues: pageIssues, preview: bodyText.slice(0, 300) });
    } catch (e) {
      results.push({ page: p.name, url: p.url, finalUrl: '?', issues: [`EXCEPTION: ${e.message}`], preview: '' });
    }
  }

  // Now check a patient detail page if we have an ID
  try {
    await page.goto(`${BASE}/patients`, { waitUntil: 'domcontentloaded', timeout: 15000 });
    await page.waitForTimeout(3000);
    // Click first patient row
    const firstRow = page.locator('table tbody tr').first();
    if (await firstRow.count() > 0) {
      await firstRow.click();
      await page.waitForTimeout(3000);
      const patientUrl = page.url();
      await page.screenshot({ path: `${SCREENSHOTS}/patient_detail.png`, fullPage: true });
      results.push({ page: 'patient_detail', url: patientUrl, finalUrl: patientUrl, issues: [], preview: '' });

      // Try sub-pages
      if (patientUrl.match(/\/patients\/[^/]+/)) {
        const patientId = patientUrl.split('/patients/')[1].split('/')[0];
        for (const sub of ['timeline', 'chart', 'treatment-plans', 'media', 'lab-orders', 'radiology']) {
          const subUrl = `/patients/${patientId}/${sub}`;
          await page.goto(`${BASE}${subUrl}`, { waitUntil: 'domcontentloaded', timeout: 10000 }).catch(() => {});
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS}/patient_${sub}.png`, fullPage: true });
          const subText = await page.locator('body').innerText().catch(() => '');
          const subIssues = [];
          if (page.url().includes('/login')) subIssues.push('REDIRECTED TO LOGIN');
          results.push({ page: `patient_${sub}`, url: subUrl, finalUrl: page.url(), issues: subIssues, preview: subText.slice(0, 200) });
        }
      }
    }
  } catch (e) {
    console.log('Patient detail check failed:', e.message);
  }

  // Write report
  const report = {
    timestamp: new Date().toISOString(),
    allErrors,
    failedRequests: failedReqs,
    pages: results
  };

  writeFileSync(`${SCREENSHOTS}/report.json`, JSON.stringify(report, null, 2));

  // Print summary
  console.log('\n===== AUDIT REPORT =====');
  for (const r of results) {
    const status = r.issues.length > 0 ? '❌' : '✅';
    console.log(`${status} ${r.page} (${r.url})`);
    if (r.issues.length > 0) r.issues.forEach(i => console.log(`   → ${i}`));
    if (r.finalUrl !== `${BASE}${r.url}` && !r.url.includes('[')) {
      console.log(`   → Redirected to: ${r.finalUrl}`);
    }
  }

  console.log('\n===== CONSOLE ERRORS =====');
  for (const [url, errs] of Object.entries(allErrors)) {
    console.log(`\n[${url}]`);
    errs.forEach(e => console.log(`  ⚠ ${e}`));
  }

  await browser.close();
})();
