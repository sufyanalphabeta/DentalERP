"use client";

import { useState } from "react";
import { Plus, Download, Trash2, Edit, Search, Filter, RefreshCw } from "lucide-react";

import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";
import { Textarea } from "@/components/ui/Textarea";
import { Checkbox } from "@/components/ui/Checkbox";
import { Badge } from "@/components/ui/Badge";
import { Alert } from "@/components/ui/Alert";
import { FormField } from "@/components/ui/FormField";
import { Spinner } from "@/components/ui/Spinner";
import { Skeleton, SkeletonRow } from "@/components/ui/Skeleton";
import { Table, Thead, Tbody, Th, Td, Tr, TableEmpty } from "@/components/ui/Table";
import { Pagination } from "@/components/ui/Pagination";
import { PageHeader } from "@/components/ui/PageHeader";
import {
  FormDialog,
  ConfirmDialog,
  DetailDialog,
  DetailGrid,
  DetailItem,
} from "@/components/ui/Dialog";
import {
  InvoiceStatusBadge,
  AppointmentStatusBadge,
  ExpenseStatusBadge,
  SupplierStatusBadge,
  ItemStatusBadge,
  InsuranceClaimStatusBadge,
} from "@/components/shared/StatusBadge";

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="mb-10">
      <h2 className="text-base font-semibold text-[var(--c-text-primary)] mb-4 pb-2 border-b border-[var(--c-border)]">
        {title}
      </h2>
      {children}
    </section>
  );
}

function Row({ children, wrap }: { children: React.ReactNode; wrap?: boolean }) {
  return (
    <div className={`flex items-center gap-3 ${wrap ? "flex-wrap" : ""} mb-3`}>
      {children}
    </div>
  );
}

function Label({ children }: { children: React.ReactNode }) {
  return (
    <span className="text-[11px] font-medium text-[var(--c-text-secondary)] uppercase tracking-wide w-24 shrink-0">
      {children}
    </span>
  );
}

const SAMPLE_ROWS = [
  { id: "INV-0001", patient: "أحمد محمد علي", amount: "٤٥٠.٠٠", status: "Paid", date: "٢٠٢٦-٠٦-٢٢" },
  { id: "INV-0002", patient: "فاطمة عبدالله", amount: "١٢٠.٠٠", status: "PartiallyPaid", date: "٢٠٢٦-٠٦-٢١" },
  { id: "INV-0003", patient: "خالد يوسف", amount: "٨٧٥.٥٠", status: "Posted", date: "٢٠٢٦-٠٦-٢٠" },
  { id: "INV-0004", patient: "نورة السعيد", amount: "٢٢٠.٠٠", status: "Draft", date: "٢٠٢٦-٠٦-١٩" },
  { id: "INV-0005", patient: "محمد إبراهيم", amount: "٣٣٠.٠٠", status: "Cancelled", date: "٢٠٢٦-٠٦-١٨" },
];

export default function DesignSystemPage() {
  const [showForm, setShowForm] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [page, setPage] = useState(1);
  const [checked, setChecked] = useState(false);
  const [hasError, setHasError] = useState(false);

  return (
    <div className="max-w-5xl mx-auto px-6 py-8" dir="rtl">
      <PageHeader
        title="نظام التصميم"
        breadcrumbs={[
          { label: "الرئيسية", href: "/" },
          { label: "نظام التصميم" },
        ]}
        actions={
          <>
            <Button variant="secondary" size="sm" iconStart={Download}>
              تصدير
            </Button>
            <Button variant="primary" size="sm" iconStart={Plus}>
              إضافة
            </Button>
          </>
        }
      />

      {/* ── Color Palette ─────────────────────────────────────────────── */}
      <Section title="لوحة الألوان — Color Palette">
        <div className="flex flex-wrap gap-3">
          {[
            ["--c-brand", "Brand"],
            ["--c-success", "Success"],
            ["--c-warning", "Warning"],
            ["--c-danger", "Danger"],
            ["--c-info", "Info"],
            ["--c-neutral", "Neutral"],
            ["--c-surface", "Surface"],
            ["--c-canvas", "Canvas"],
          ].map(([token, name]) => (
            <div key={token} className="flex flex-col items-center gap-1.5">
              <div
                className="w-14 h-14 rounded-lg border border-[var(--c-border)] shadow-sm"
                style={{ background: `var(${token})` }}
              />
              <span className="text-[10px] text-[var(--c-text-secondary)]">{name}</span>
            </div>
          ))}
        </div>
      </Section>

      {/* ── Buttons ───────────────────────────────────────────────────── */}
      <Section title="الأزرار — Buttons">
        <Row>
          <Label>Variants</Label>
          <Button variant="primary">أساسي</Button>
          <Button variant="secondary">ثانوي</Button>
          <Button variant="danger">حذف</Button>
          <Button variant="ghost">خفي</Button>
        </Row>
        <Row>
          <Label>Sizes</Label>
          <Button variant="primary" size="sm">صغير</Button>
          <Button variant="primary" size="md">متوسط</Button>
          <Button variant="primary" size="lg">كبير</Button>
        </Row>
        <Row>
          <Label>Icons</Label>
          <Button variant="primary" iconStart={Plus}>إضافة صنف</Button>
          <Button variant="secondary" iconStart={Filter}>فلتر</Button>
          <Button variant="secondary" iconEnd={Download}>تصدير PDF</Button>
          <Button variant="ghost" iconStart={RefreshCw}>تحديث</Button>
        </Row>
        <Row>
          <Label>States</Label>
          <Button variant="primary" loading>جارٍ الحفظ...</Button>
          <Button variant="secondary" disabled>معطّل</Button>
        </Row>
      </Section>

      {/* ── Badges ────────────────────────────────────────────────────── */}
      <Section title="الشارات — Badges">
        <Row wrap>
          <Badge variant="success">نشط</Badge>
          <Badge variant="warning">معلق</Badge>
          <Badge variant="danger">ملغى</Badge>
          <Badge variant="info">مؤكد</Badge>
          <Badge variant="neutral">مسودة</Badge>
          <Badge variant="brand">مُرحَّل</Badge>
        </Row>
      </Section>

      {/* ── Status Badges ─────────────────────────────────────────────── */}
      <Section title="شارات الحالة — Status Badges">
        <div className="space-y-3">
          <Row wrap>
            <Label>فاتورة</Label>
            {["Draft", "Posted", "Paid", "PartiallyPaid", "Cancelled"].map((s) => (
              <InvoiceStatusBadge key={s} value={s} />
            ))}
          </Row>
          <Row wrap>
            <Label>موعد</Label>
            {["Scheduled", "Confirmed", "InProgress", "Completed", "NoShow", "Cancelled"].map((s) => (
              <AppointmentStatusBadge key={s} value={s} />
            ))}
          </Row>
          <Row wrap>
            <Label>مصروف</Label>
            {["Draft", "Posted", "Cancelled"].map((s) => (
              <ExpenseStatusBadge key={s} value={s} />
            ))}
          </Row>
          <Row wrap>
            <Label>مورّد</Label>
            {["Active", "Inactive"].map((s) => (
              <SupplierStatusBadge key={s} value={s} />
            ))}
          </Row>
          <Row wrap>
            <Label>صنف</Label>
            {["Active", "LowStock", "OutOfStock", "Inactive"].map((s) => (
              <ItemStatusBadge key={s} value={s} />
            ))}
          </Row>
          <Row wrap>
            <Label>تأمين</Label>
            {["Pending", "Submitted", "Approved", "PartiallyApproved", "Rejected"].map((s) => (
              <InsuranceClaimStatusBadge key={s} value={s} />
            ))}
          </Row>
        </div>
      </Section>

      {/* ── Form Controls ─────────────────────────────────────────────── */}
      <Section title="حقول الإدخال — Form Controls">
        <div className="grid grid-cols-2 gap-4 max-w-2xl">
          <FormField label="اسم المريض" required labelFor="demo-name">
            <Input id="demo-name" placeholder="أدخل الاسم الكامل" />
          </FormField>
          <FormField
            label="رقم الهاتف"
            error={hasError ? "رقم الهاتف غير صحيح" : undefined}
            labelFor="demo-phone"
          >
            <Input
              id="demo-phone"
              placeholder="05xxxxxxxx"
              error={hasError}
              onBlur={() => setHasError(true)}
            />
          </FormField>
          <FormField label="التخصص" labelFor="demo-specialty">
            <Select id="demo-specialty">
              <option value="">اختر التخصص</option>
              <option>تقويم الأسنان</option>
              <option>علاج الجذور</option>
              <option>جراحة الفم</option>
            </Select>
          </FormField>
          <FormField label="ملاحظات" hint="اختياري" labelFor="demo-notes">
            <Textarea id="demo-notes" placeholder="أي ملاحظات إضافية..." rows={3} />
          </FormField>
          <div className="flex items-center gap-6 col-span-2">
            <Checkbox
              label="متابعة لاحقاً"
              description="إضافة هذا المريض لقائمة المتابعة"
              checked={checked}
              onChange={(e) => setChecked(e.target.checked)}
            />
            <Checkbox label="إرسال تذكير SMS" checked={false} onChange={() => {}} />
          </div>
        </div>
      </Section>

      {/* ── Alerts ────────────────────────────────────────────────────── */}
      <Section title="التنبيهات — Alerts">
        <div className="space-y-3 max-w-2xl">
          <Alert variant="info" title="تحديث النظام">
            سيكون النظام في وضع الصيانة يوم الجمعة من الساعة ٢ إلى ٤ صباحاً.
          </Alert>
          <Alert variant="success" title="تم الحفظ">
            تم حفظ بيانات المريض بنجاح.
          </Alert>
          <Alert variant="warning">
            المخزون منخفض: كمية القفازات أقل من الحد الأدنى المطلوب.
          </Alert>
          <Alert variant="danger" title="خطأ في العملية" onDismiss={() => {}}>
            لا يمكن حذف المورد لأن لديه رصيد مستحق.
          </Alert>
        </div>
      </Section>

      {/* ── Loading States ─────────────────────────────────────────────── */}
      <Section title="حالات التحميل — Loading States">
        <Row>
          <Label>Spinners</Label>
          <Spinner size="sm" />
          <Spinner size="md" />
          <Spinner size="lg" />
        </Row>
        <Row>
          <Label>Skeleton</Label>
          <div className="flex-1 max-w-xs space-y-2">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
            <SkeletonRow cols={[30, 40, 20]} />
          </div>
        </Row>
      </Section>

      {/* ── Table ──────────────────────────────────────────────────────── */}
      <Section title="الجداول — Tables">
        <Table>
          <Thead>
            <tr>
              <Th>رقم الفاتورة</Th>
              <Th>المريض</Th>
              <Th>التاريخ</Th>
              <Th>الحالة</Th>
              <Th className="text-end">المبلغ</Th>
              <Th>إجراءات</Th>
            </tr>
          </Thead>
          <Tbody>
            {SAMPLE_ROWS.map((row) => (
              <Tr key={row.id} onClick={() => {}}>
                <Td mono>{row.id}</Td>
                <Td>{row.patient}</Td>
                <Td>{row.date}</Td>
                <Td>
                  <InvoiceStatusBadge value={row.status} />
                </Td>
                <Td amount>{row.amount} ر.س</Td>
                <Td>
                  <div className="flex gap-2">
                    <Button variant="ghost" size="sm" iconStart={Edit} />
                    <Button variant="ghost" size="sm" iconStart={Trash2} />
                  </div>
                </Td>
              </Tr>
            ))}
          </Tbody>
        </Table>
        <div className="mt-2">
          <Pagination page={page} pageSize={5} total={47} onPage={setPage} />
        </div>

        <div className="mt-4">
          <Table>
            <Thead>
              <tr>
                <Th>الرقم</Th>
                <Th>الاسم</Th>
                <Th>الحالة</Th>
              </tr>
            </Thead>
            <Tbody>
              <TableEmpty colSpan={3} filtered={false} />
            </Tbody>
          </Table>
        </div>

        <div className="mt-4">
          <Table>
            <Thead>
              <tr>
                <Th>الرقم</Th>
                <Th>الاسم</Th>
                <Th>الحالة</Th>
              </tr>
            </Thead>
            <Tbody>
              <TableEmpty colSpan={3} filtered />
            </Tbody>
          </Table>
        </div>
      </Section>

      {/* ── Dialogs ────────────────────────────────────────────────────── */}
      <Section title="مربعات الحوار — Dialogs">
        <Row>
          <Button variant="primary" iconStart={Plus} onClick={() => setShowForm(true)}>
            Type A — نموذج
          </Button>
          <Button variant="danger" onClick={() => setShowConfirm(true)}>
            Type B — تأكيد
          </Button>
          <Button variant="secondary" onClick={() => setShowDetail(true)}>
            Type C — تفاصيل
          </Button>
        </Row>

        <FormDialog
          open={showForm}
          onClose={() => setShowForm(false)}
          title="إضافة مريض جديد"
          footer={
            <>
              <Button variant="secondary" onClick={() => setShowForm(false)}>
                إلغاء
              </Button>
              <Button variant="primary">حفظ</Button>
            </>
          }
        >
          <div className="grid grid-cols-2 gap-4">
            <FormField label="الاسم الكامل" required labelFor="fd-name">
              <Input id="fd-name" placeholder="أدخل الاسم" />
            </FormField>
            <FormField label="رقم الهاتف" labelFor="fd-phone">
              <Input id="fd-phone" placeholder="05xxxxxxxx" />
            </FormField>
            <FormField label="الجنس" labelFor="fd-gender">
              <Select id="fd-gender">
                <option value="">اختر</option>
                <option>ذكر</option>
                <option>أنثى</option>
              </Select>
            </FormField>
            <FormField label="تاريخ الميلاد" labelFor="fd-dob">
              <Input id="fd-dob" type="date" />
            </FormField>
          </div>
        </FormDialog>

        <ConfirmDialog
          open={showConfirm}
          onClose={() => setShowConfirm(false)}
          onConfirm={() => setShowConfirm(false)}
          title="حذف المريض"
          message="هل أنت متأكد من حذف هذا المريض؟ لا يمكن التراجع عن هذا الإجراء."
          variant="danger"
        />

        <DetailDialog
          open={showDetail}
          onClose={() => setShowDetail(false)}
          title="فاتورة INV-0001"
          subtitle="أحمد محمد علي"
          badge={<InvoiceStatusBadge value="Paid" />}
          actions={
            <>
              <Button variant="secondary" size="sm" iconStart={Download}>
                طباعة
              </Button>
            </>
          }
        >
          <DetailGrid>
            <DetailItem label="رقم الفاتورة" value="INV-0001" />
            <DetailItem label="التاريخ" value="٢٢ يونيو ٢٠٢٦" />
            <DetailItem label="المريض" value="أحمد محمد علي" />
            <DetailItem label="الطبيب" value="د. سارة الأحمد" />
            <DetailItem label="المبلغ الإجمالي" value="٤٥٠.٠٠ ر.س" />
            <DetailItem label="المدفوع" value="٤٥٠.٠٠ ر.س" />
            <DetailItem label="المتبقي" value="٠.٠٠ ر.س" />
            <DetailItem label="طريقة الدفع" value="نقداً" />
            <DetailItem label="الخدمات" value="تنظيف الأسنان، حشو ضرس" full />
          </DetailGrid>
        </DetailDialog>
      </Section>

      {/* ── Search bar example ─────────────────────────────────────────── */}
      <Section title="شريط البحث — Search Bar Pattern">
        <div className="flex items-center gap-3 max-w-2xl">
          <div className="relative flex-1">
            <Search
              size={14}
              className="absolute top-1/2 -translate-y-1/2 end-3 text-[var(--c-text-secondary)]"
            />
            <Input placeholder="بحث عن مريض، فاتورة، أو موعد..." className="pe-9" />
          </div>
          <Button variant="secondary" iconStart={Filter}>فلتر</Button>
          <Button variant="secondary" iconStart={RefreshCw} />
        </div>
      </Section>
    </div>
  );
}
