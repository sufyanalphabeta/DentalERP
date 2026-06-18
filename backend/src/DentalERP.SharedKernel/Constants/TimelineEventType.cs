namespace DentalERP.SharedKernel.Constants;

public static class TimelineEventType
{
    // Financial
    public const string InvoiceCreated = "invoice_created";
    public const string InvoiceConfirmed = "invoice_confirmed";
    public const string PaymentReceived = "payment_received";
    public const string InvoiceCancelled = "invoice_cancelled";

    // Laboratory
    public const string LabOrderCreated = "lab_order_created";
    public const string LabOrderSent = "lab_order_sent";
    public const string LabResultReceived = "lab_result_received";
    public const string LabOrderCompleted = "lab_order_completed";

    // Radiology
    public const string RadiologyOrderCreated = "radiology_order_created";
    public const string RadiologyImaged = "radiology_imaged";
    public const string RadiologyReportSaved = "radiology_report_saved";
    public const string RadiologyOrderCompleted = "radiology_order_completed";

    // Insurance
    public const string InsuranceClaimCreated = "insurance_claim_created";
    public const string InsuranceClaimSubmitted = "insurance_claim_submitted";
    public const string InsurancePaymentReceived = "insurance_payment_received";
    public const string InsuranceClaimRejected = "insurance_claim_rejected";
}
