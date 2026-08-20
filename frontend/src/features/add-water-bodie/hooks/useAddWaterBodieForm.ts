"use client";

import { useState } from "react";

import { initialAddWaterBodieForm } from "@/features/add-water-bodie/constants/addWaterBodieOptions";
import type {
  AddWaterBodieErrors,
  AddWaterBodieFieldName,
  AddWaterBodieFormState,
  RequiredWaterBodieField,
} from "@/features/add-water-bodie/types/addWaterBodie";

const requiredFieldMessages: Record<RequiredWaterBodieField, string> = {
  name: "Informe o nome do corpo hídrico.",
  location: "Informe a localização do corpo hídrico.",
  size: "Informe o tamanho do corpo hídrico.",
};

export function useAddWaterBodieForm() {
  const [form, setForm] = useState<AddWaterBodieFormState>(
    initialAddWaterBodieForm
  );
  const [errors, setErrors] = useState<AddWaterBodieErrors>({});

  function updateField(name: AddWaterBodieFieldName, value: string) {
    setForm((currentForm) => ({
      ...currentForm,
      [name]: value,
    }));

    if (name in requiredFieldMessages) {
      setErrors((currentErrors) => {
        const nextErrors = { ...currentErrors };
        delete nextErrors[name as RequiredWaterBodieField];
        return nextErrors;
      });
    }
  }

  function updatePrivateStatus(ehPrivado: boolean) {
    setForm((currentForm) => ({
      ...currentForm,
      ehPrivado,
    }));
  }

  function validateForm() {
    const nextErrors: AddWaterBodieErrors = {};

    (Object.keys(requiredFieldMessages) as RequiredWaterBodieField[]).forEach(
      (fieldName) => {
        if (!form[fieldName].trim()) {
          nextErrors[fieldName] = requiredFieldMessages[fieldName];
        }
      }
    );

    const parsedSize = Number(form.size.replace(",", "."));

    if (form.size.trim() && (!Number.isFinite(parsedSize) || parsedSize <= 0)) {
      nextErrors.size = "O tamanho deve ser um número maior que zero.";
    }

    setErrors(nextErrors);
    return nextErrors;
  }

  function resetForm() {
    setForm(initialAddWaterBodieForm);
    setErrors({});
  }

  return {
    form,
    errors,
    updateField,
    updatePrivateStatus,
    validateForm,
    resetForm,
  };
}
