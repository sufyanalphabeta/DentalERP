export interface PatientSummary {
  id: string;
  fileNumber: string;
  fullName: string;
  phone: string;
  gender?: string;
  age?: number;
  isActive: boolean;
}

export interface PatientDetail extends PatientSummary {
  phone2?: string;
  email?: string;
  dateOfBirth?: string;
  address?: string;
  nationalId?: string;
  bloodType?: string;
  allergies?: string;
  chronicDiseases?: string;
  notes?: string;
  createdAt: string;
}

export interface GetPatientsResponse {
  items: PatientSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AppointmentItem {
  id: string;
  patientId: string;
  patientName: string;
  patientPhone: string;
  doctorId: string;
  scheduledAt: string;
  durationMinutes: number;
  status: AppointmentStatus;
  typeName?: string;
  typeNameAr?: string;
  typeColor?: string;
  chiefComplaint?: string;
}

export type AppointmentStatus =
  | "Scheduled"
  | "Confirmed"
  | "InProgress"
  | "Completed"
  | "Cancelled"
  | "NoShow";

export interface QueueEntryItem {
  id: string;
  tokenNumber: number;
  patientId: string;
  patientName: string;
  patientPhone: string;
  doctorId?: string;
  status: QueueStatus;
  checkInAt: string;
  calledAt?: string;
  startedAt?: string;
  completedAt?: string;
}

export type QueueStatus =
  | "Waiting"
  | "Called"
  | "InProgress"
  | "Completed"
  | "Skipped";

export interface GetQueueResponse {
  date: string;
  entries: QueueEntryItem[];
}
