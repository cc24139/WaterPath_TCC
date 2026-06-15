"use client";

import { useEffect, useRef, useState } from "react";

import {
  defaultImagePreviewUrl,
  initialAddAnalysisForm,
} from "@/features/add-analysis/constants/addAnalysisOptions";
import type {
  AddAnalysisFieldName,
  AddAnalysisFormState,
} from "@/features/add-analysis/types/addAnalysis";

export function useAddAnalysisForm() {
  const [form, setForm] = useState<AddAnalysisFormState>(
    initialAddAnalysisForm
  );
  const objectUrlRef = useRef<string | null>(null);

  useEffect(() => {
    return () => {
      if (objectUrlRef.current) {
        URL.revokeObjectURL(objectUrlRef.current);
      }
    };
  }, []);

  function updateField(name: AddAnalysisFieldName, value: string) {
    setForm((currentForm) => ({
      ...currentForm,
      [name]: value,
    }));
  }

  function updateImage(file: File) {
    if (!file.type.startsWith("image/")) {
      return false;
    }

    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
    }

    const previewUrl = URL.createObjectURL(file);
    objectUrlRef.current = previewUrl;

    setForm((currentForm) => ({
      ...currentForm,
      imageName: file.name,
      imagePreviewUrl: previewUrl,
    }));

    return true;
  }

  function removeImage() {
    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
      objectUrlRef.current = null;
    }

    setForm((currentForm) => ({
      ...currentForm,
      imageName: "",
      imagePreviewUrl: defaultImagePreviewUrl,
    }));
  }

  function resetForm() {
    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
      objectUrlRef.current = null;
    }

    setForm(initialAddAnalysisForm);
  }

  function canSubmit() {
    return Boolean(
      form.waterBody &&
        form.analysisDate &&
        form.collectionPoint &&
        form.responsible &&
        form.analysisType
    );
  }

  return {
    form,
    updateField,
    updateImage,
    removeImage,
    resetForm,
    canSubmit,
  };
}
