import { chromium } from 'playwright';
import fs from 'fs';
import path from 'path';

const BASE = 'http://localhost';
const OUTDIR = './audit_screenshots';
if (!fs.existsSync(OUTDIR)) fs.mkdirSync(OUTDIR, { recursive: true });

const PAGES = [
  { path: '/', name: 'Dashboard' },
  { path: '/reception', name: 'Reception' },
  { path: '/clinical/workspace', name: 'Doctor Workspace' },
  { path: '/finance/cashier', name: 'Cashier' },
  { path: '/patients', name: 'Patients List' },
  { path: '/patients/new', name: 'New Patient' },
  { path: '/appointments', name: 'Appointments' },
  { path: '/queue', name: 'Queue' },
  { path: '/finance/invoices', name: 'Invoices' },
  { path: '/finance/invoices/new', name: 'New Invoice' },
  { path: '/finance/installments', name: 'Installments' },
  { path: '/finance/doctors', name: 'Doctor Accounts' },
  { path: '/treasury/movements', name: 'Cash Movements' },
  { path: '/treasury/transfers', name: 'Transfers' },
  { path: '/inventory/alerts', name: 'Stock Alerts' },
  { path: '/inventory/items', name: 'Items' },
  { path: '/inventory/categories', name: 'Item Categories' },
  { path: '/inventory/warehouses', name: 'Warehouses' },
  { path: '/inventory/movements', name: 'Stock Movements' },
  { path: '/purchasing/suppliers', name: 'Suppliers' },
  { path: '/purchasing/orders', name: 'Purchase Orders' },
  { path: '/purchasing/returns', name: 'Purchase Returns' },
  { path: '/expenses', name: 'Expenses' },
  { path: '/expenses/categories', name: 'Expense Categories' },
  { path: '/assets', name: 'Assets' },
  { path: '/assets/categories', name: 'Asset Categories' },
  { path: '/lab/orders', name: 'Lab Orders' },
  { path: '/lab/external-labs', name: 'External Labs' },
  { path: '/radiology/orders', name: 'Radiology Orders' },
  { path: '/reports', name: 'Reports' },
  { path: '/settings/users', name: 'Users' },
  { path: '/settings/roles', name: 'Roles' },
  { path: '/settings/doctors', name: 'Settings Doctors' },
  { path: '/settings/services', name: 'Services' },
  { path: '/settings/services/categories', name: 'Service Categories' },
  { path: '/settings/vaults', name: 'Vaults' },
  { path: '/settings/system', name: 'System Settings' },
];

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: 'networkidle', timeout: 20000 });
  const user = await page.$('input[type="text"]') || await page.$('input[name="username"]');
  const pass = await page.$('input[type="password"]');
  await user.fill('admin');
  await pass.fill('Admin@123');
  await page.click('button[type="submit"]');
  await page.waitForURL(`${BASE}/`, { timeout: 15000 });
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await ctx.newPage();

  const results = [];

  console.log('Logging in...');
  try {
    await login(page);
    console.log('Logged in OK\n');
  } catch(e) {
    console.error('Login FAILED:', e.message);
    process.exit(1);
  }

  for (const pg of PAGES) {
    const pageErrors = [];
    const apiFails = [];

    page.on('response', resp => {
      const u = resp.url();
      if (u.includes('/api/') && [404,405,500].includes(resp.status())) {
        apiFails.push(`${resp.status()} ${resp.request().method()} ${u.replace('http://localhost','')} `);
      }
    });
    page.on('console', msg => {
      if (msg.type() === 'error') {
        const t = msg.text();
        if (t.includes('TypeError') || t.includes('Cannot read') || t.includes('toFixed')) {
          pageErrors.push(t.slice(0,200));
        }
      }
    });

    try {
      await page.goto(`${BASE}${pg.path}`, { waitUntil: 'networkidle', timeout: 18000 });
      await page.waitForTimeout(2000);

      const ssFile = `${OUTDIR}/${pg.path.replace(/\//g,'_').replace(/^_/,'') || 'home'}.png`;
      await page.screenshot({ path: ssFile });

      const bodyText = await page.locator('body').innerText().catch(() => '');
      const isBlank = bodyText.trim().length < 40;
      const isError404 = bodyText.includes('404') || bodyText.includes('not found');

      const uniqueApi = [...new Set(apiFails)];
      const status = pageErrors.length ? 'CRASH' : isBlank ? 'BLANK' : uniqueApi.length ? 'API_ERR' : 'OK';

      results.push({ path: pg.path, name: pg.name, status, errors: pageErrors, api: uniqueApi });

      const icon = status === 'OK' ? '✅' : status === 'API_ERR' ? '⚠️' : '❌';
      process.stdout.write(`${icon} ${pg.path}\n`);
      if (pageErrors.length) pageErrors.forEach(e => process.stdout.write(`     💥 ${e.slice(0,120)}\n`));
      if (uniqueApi.length) uniqueApi.forEach(e => process.stdout.write(`     🔴 ${e}\n`));

    } catch(e) {
      results.push({ path: pg.path, name: pg.name, status: 'TIMEOUT', errors: [e.message], api: [] });
      process.stdout.write(`❌ ${pg.path} TIMEOUT: ${e.message.slice(0,80)}\n`);
    }

    page.removeAllListeners('response');
    page.removeAllListeners('console');
  }

  await browser.close();

  const ok = results.filter(r=>r.status==='OK').length;
  const apiErr = results.filter(r=>r.status==='API_ERR');
  const broken = results.filter(r=>['CRASH','BLANK','TIMEOUT'].includes(r.status));

  console.log(`\n${'='.repeat(60)}`);
  console.log(`SUMMARY: ✅ ${ok} OK | ⚠️  ${apiErr.length} API errors | ❌ ${broken.length} broken`);
  console.log('='.repeat(60));
  if (broken.length) {
    console.log('\nBROKEN PAGES:');
    broken.forEach(r => { console.log(`  ❌ ${r.path} (${r.status})`); r.errors.forEach(e=>console.log(`     ${e.slice(0,120)}`)); });
  }
  if (apiErr.length) {
    console.log('\nAPI ERRORS:');
    apiErr.forEach(r => { console.log(`  ⚠️  ${r.path}`); r.api.forEach(e=>console.log(`     ${e}`)); });
  }

  fs.writeFileSync(`${OUTDIR}/_results.json`, JSON.stringify(results, null, 2));
})();
