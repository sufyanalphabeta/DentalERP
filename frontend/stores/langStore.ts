import { create } from "zustand";
import { persist } from "zustand/middleware";

export type Lang = "ar" | "en";

interface LangStore {
  lang: Lang;
  setLang: (l: Lang) => void;
}

export const useLangStore = create<LangStore>()(
  persist(
    (set) => ({
      lang: "ar",
      setLang: (lang) => set({ lang }),
    }),
    { name: "dental-lang" }
  )
);
