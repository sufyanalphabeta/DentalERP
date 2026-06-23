// Typed permission constants matching the backend permission catalog.
// Format: Module.Screen.Action

export const P = {
  // ── Dashboard ────────────────────────────────────────────────────────────
  Dashboard: {
    Overview:    { View: 'Dashboard.Overview.View' },
    Revenue:     { View: 'Dashboard.Revenue.View' },
    Operations:  { View: 'Dashboard.Operations.View' },
    Financial:   { View: 'Dashboard.Financial.View' },
    Inventory:   { View: 'Dashboard.Inventory.View' },
    Executive:   { View: 'Dashboard.Executive.View' },
  },

  // ── Patients ─────────────────────────────────────────────────────────────
  Patients: {
    Patients: {
      View:        'Patients.Patients.View',
      Create:      'Patients.Patients.Create',
      Edit:        'Patients.Patients.Edit',
      Delete:      'Patients.Patients.Delete',
      Print:       'Patients.Patients.Print',
      ExportExcel: 'Patients.Patients.ExportExcel',
      ExportPdf:   'Patients.Patients.ExportPdf',
    },
  },

  // ── Appointments ─────────────────────────────────────────────────────────
  Appointments: {
    Appointments: {
      View:        'Appointments.Appointments.View',
      Create:      'Appointments.Appointments.Create',
      Edit:        'Appointments.Appointments.Edit',
      Delete:      'Appointments.Appointments.Delete',
      Reschedule:  'Appointments.Appointments.Reschedule',
      Cancel:      'Appointments.Appointments.Cancel',
      Print:       'Appointments.Appointments.Print',
    },
    Queue: {
      View:   'Appointments.Queue.View',
      Create: 'Appointments.Queue.Create',
      Edit:   'Appointments.Queue.Edit',
      Delete: 'Appointments.Queue.Delete',
    },
  },

  // ── Clinical ─────────────────────────────────────────────────────────────
  Clinical: {
    Workspace:      { View: 'Clinical.Workspace.View' },
    TreatmentPlans: {
      View:    'Clinical.TreatmentPlans.View',
      Create:  'Clinical.TreatmentPlans.Create',
      Edit:    'Clinical.TreatmentPlans.Edit',
      Delete:  'Clinical.TreatmentPlans.Delete',
      Approve: 'Clinical.TreatmentPlans.Approve',
      Print:   'Clinical.TreatmentPlans.Print',
    },
    DentalChart: {
      View: 'Clinical.DentalChart.View',
      Edit: 'Clinical.DentalChart.Edit',
    },
    Procedures: {
      View:   'Clinical.Procedures.View',
      Create: 'Clinical.Procedures.Create',
      Edit:   'Clinical.Procedures.Edit',
      Delete: 'Clinical.Procedures.Delete',
    },
    Files: {
      View:   'Clinical.Files.View',
      Upload: 'Clinical.Files.Upload',
      Delete: 'Clinical.Files.Delete',
    },
  },

  // ── Laboratory ───────────────────────────────────────────────────────────
  Lab: {
    Orders: {
      View:   'Lab.Orders.View',
      Create: 'Lab.Orders.Create',
      Edit:   'Lab.Orders.Edit',
      Delete: 'Lab.Orders.Delete',
      Print:  'Lab.Orders.Print',
    },
    Results: {
      View: 'Lab.Results.View',
      Edit: 'Lab.Results.Edit',
    },
    ExternalLabs: {
      View:   'Lab.ExternalLabs.View',
      Create: 'Lab.ExternalLabs.Create',
      Edit:   'Lab.ExternalLabs.Edit',
    },
  },

  // ── Radiology ────────────────────────────────────────────────────────────
  Radiology: {
    Orders: {
      View:   'Radiology.Orders.View',
      Create: 'Radiology.Orders.Create',
      Edit:   'Radiology.Orders.Edit',
      Delete: 'Radiology.Orders.Delete',
      Print:  'Radiology.Orders.Print',
    },
    Images: {
      View:   'Radiology.Images.View',
      Upload: 'Radiology.Images.Upload',
    },
  },

  // ── Financial ────────────────────────────────────────────────────────────
  Financial: {
    CashierDesk:   { View: 'Financial.CashierDesk.View' },
    Invoices: {
      View:      'Financial.Invoices.View',
      Create:    'Financial.Invoices.Create',
      Edit:      'Financial.Invoices.Edit',
      Delete:    'Financial.Invoices.Delete',
      Print:     'Financial.Invoices.Print',
      ExportPdf: 'Financial.Invoices.ExportPdf',
      Confirm:   'Financial.Invoices.Confirm',
      Cancel:    'Financial.Invoices.Cancel',
      Refund:    'Financial.Invoices.Refund',
    },
    Installments: {
      View:   'Financial.Installments.View',
      Create: 'Financial.Installments.Create',
      Edit:   'Financial.Installments.Edit',
      Delete: 'Financial.Installments.Delete',
    },
    Payments: {
      View:   'Financial.Payments.View',
      Create: 'Financial.Payments.Create',
      Delete: 'Financial.Payments.Delete',
    },
    Treasury: {
      View:     'Financial.Treasury.View',
      Create:   'Financial.Treasury.Create',
      Edit:     'Financial.Treasury.Edit',
      Transfer: 'Financial.Treasury.Transfer',
    },
    Expenses: {
      View:        'Financial.Expenses.View',
      Create:      'Financial.Expenses.Create',
      Edit:        'Financial.Expenses.Edit',
      Delete:      'Financial.Expenses.Delete',
      ExportPdf:   'Financial.Expenses.ExportPdf',
      ExportExcel: 'Financial.Expenses.ExportExcel',
    },
    Doctors: {
      View: 'Financial.Doctors.View',
      Edit: 'Financial.Doctors.Edit',
    },
  },

  // ── Insurance ────────────────────────────────────────────────────────────
  Insurance: {
    Companies: {
      View:   'Insurance.Companies.View',
      Create: 'Insurance.Companies.Create',
      Edit:   'Insurance.Companies.Edit',
      Delete: 'Insurance.Companies.Delete',
    },
    Claims: {
      View:      'Insurance.Claims.View',
      Create:    'Insurance.Claims.Create',
      Edit:      'Insurance.Claims.Edit',
      Delete:    'Insurance.Claims.Delete',
      Print:     'Insurance.Claims.Print',
      ExportPdf: 'Insurance.Claims.ExportPdf',
      Approve:   'Insurance.Claims.Approve',
      Cancel:    'Insurance.Claims.Cancel',
    },
    Receivables: { View: 'Insurance.Receivables.View' },
  },

  // ── Inventory ────────────────────────────────────────────────────────────
  Inventory: {
    Items: {
      View:        'Inventory.Items.View',
      Create:      'Inventory.Items.Create',
      Edit:        'Inventory.Items.Edit',
      Delete:      'Inventory.Items.Delete',
      ExportExcel: 'Inventory.Items.ExportExcel',
    },
    Movements: {
      View:   'Inventory.Movements.View',
      Create: 'Inventory.Movements.Create',
    },
    Stocktake: {
      View:   'Inventory.Stocktake.View',
      Create: 'Inventory.Stocktake.Create',
    },
    Alerts: { View: 'Inventory.Alerts.View' },
  },

  // ── Purchasing ───────────────────────────────────────────────────────────
  Purchasing: {
    Suppliers: {
      View:   'Purchasing.Suppliers.View',
      Create: 'Purchasing.Suppliers.Create',
      Edit:   'Purchasing.Suppliers.Edit',
    },
    Orders: {
      View:    'Purchasing.Orders.View',
      Create:  'Purchasing.Orders.Create',
      Edit:    'Purchasing.Orders.Edit',
      Delete:  'Purchasing.Orders.Delete',
      Approve: 'Purchasing.Orders.Approve',
      Print:   'Purchasing.Orders.Print',
    },
    Invoices: {
      View:      'Purchasing.Invoices.View',
      Create:    'Purchasing.Invoices.Create',
      Edit:      'Purchasing.Invoices.Edit',
      Delete:    'Purchasing.Invoices.Delete',
      Print:     'Purchasing.Invoices.Print',
      ExportPdf: 'Purchasing.Invoices.ExportPdf',
    },
    Returns: {
      View:   'Purchasing.Returns.View',
      Create: 'Purchasing.Returns.Create',
    },
  },

  // ── Assets ───────────────────────────────────────────────────────────────
  Assets: {
    Assets: {
      View:   'Assets.Assets.View',
      Create: 'Assets.Assets.Create',
      Edit:   'Assets.Assets.Edit',
      Delete: 'Assets.Assets.Delete',
      Print:  'Assets.Assets.Print',
    },
    Maintenance: {
      View:   'Assets.Maintenance.View',
      Create: 'Assets.Maintenance.Create',
    },
    Categories: {
      View:   'Assets.Categories.View',
      Create: 'Assets.Categories.Create',
      Edit:   'Assets.Categories.Edit',
    },
  },

  // ── Reports ──────────────────────────────────────────────────────────────
  Reports: {
    Financial: {
      View:        'Reports.Financial.View',
      Print:       'Reports.Financial.Print',
      ExportPdf:   'Reports.Financial.ExportPdf',
      ExportExcel: 'Reports.Financial.ExportExcel',
    },
    Operational: {
      View:        'Reports.Operational.View',
      Print:       'Reports.Operational.Print',
      ExportPdf:   'Reports.Operational.ExportPdf',
      ExportExcel: 'Reports.Operational.ExportExcel',
    },
    Purchasing: {
      View:      'Reports.Purchasing.View',
      Print:     'Reports.Purchasing.Print',
      ExportPdf: 'Reports.Purchasing.ExportPdf',
    },
    Inventory: {
      View:  'Reports.Inventory.View',
      Print: 'Reports.Inventory.Print',
    },
    ARaging: {
      View:      'Reports.ARaging.View',
      ExportPdf: 'Reports.ARaging.ExportPdf',
    },
    Collections: {
      View:      'Reports.Collections.View',
      ExportPdf: 'Reports.Collections.ExportPdf',
    },
  },

  // ── IAM (Settings / Admin) ────────────────────────────────────────────────
  IAM: {
    Users: {
      View:   'IAM.Users.View',
      Create: 'IAM.Users.Create',
      Edit:   'IAM.Users.Edit',
      Delete: 'IAM.Users.Delete',
    },
    Roles: {
      View:   'IAM.Roles.View',
      Create: 'IAM.Roles.Create',
      Edit:   'IAM.Roles.Edit',
      Delete: 'IAM.Roles.Delete',
    },
    Settings: {
      View: 'IAM.Settings.View',
      Edit: 'IAM.Settings.Edit',
    },
    Services: {
      View:   'IAM.Services.View',
      Create: 'IAM.Services.Create',
      Edit:   'IAM.Services.Edit',
      Delete: 'IAM.Services.Delete',
    },
    Vaults: {
      View:   'IAM.Vaults.View',
      Create: 'IAM.Vaults.Create',
      Edit:   'IAM.Vaults.Edit',
    },
    Doctors: {
      View:   'IAM.Doctors.View',
      Create: 'IAM.Doctors.Create',
      Edit:   'IAM.Doctors.Edit',
    },
    Insurance: {
      View:   'IAM.Insurance.View',
      Create: 'IAM.Insurance.Create',
    },
  },
} as const;

export type Permission = string;
