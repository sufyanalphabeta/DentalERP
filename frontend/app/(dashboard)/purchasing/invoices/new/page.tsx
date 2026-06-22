// Re-export the [id] page component.
// When running under the /purchasing/invoices/new route, useParams() returns {}
// so params?.id is undefined, which falls back to "new" → isNew = true.
export { default } from "../[id]/page";
