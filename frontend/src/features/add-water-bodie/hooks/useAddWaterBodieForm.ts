"use client";

import { useEffect, useRef, useState } from "react";

import {
  initialAddWaterBodieForm,
  initialCollectionPoints,
} from "@/features/add-water-bodie/constants/addWaterBodieOptions";
import type {
  AddWaterBodieErrors,
  AddWaterBodieFieldName,
  AddWaterBodieFormState,
  CollectionPoint,
  RequiredWaterBodieField,
} from "@/features/add-water-bodie/types/addWaterBodie";

const maxImageSizeInBytes = 5 * 1024 * 1024;
const acceptedImageTypes = ["image/png", "image/jpeg", "image/jpg"];

const requiredFieldMessages: Record<RequiredWaterBodieField, string> = {
  name: "Informe o nome do corpo hídrico.",
  type: "Selecione o tipo de corpo hídrico.",
  city: "Informe a cidade.",
  state: "Selecione o estado.",
};

export function useAddWaterBodieForm() {
  const [form, setForm] = useState<AddWaterBodieFormState>(
    initialAddWaterBodieForm
  );
  const [errors, setErrors] = useState<AddWaterBodieErrors>({});
  const [imageError, setImageError] = useState("");
  const [collectionPoints, setCollectionPoints] = useState<CollectionPoint[]>(
    initialCollectionPoints
  );
  const objectUrlRef = useRef<string | null>(null);

  useEffect(() => {
    return () => {
      if (objectUrlRef.current) {
        URL.revokeObjectURL(objectUrlRef.current);
      }
    };
  }, []);

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

  function validateForm() {
    const nextErrors: AddWaterBodieErrors = {};

    (Object.keys(requiredFieldMessages) as RequiredWaterBodieField[]).forEach(
      (fieldName) => {
        if (!form[fieldName].trim()) {
          nextErrors[fieldName] = requiredFieldMessages[fieldName];
        }
      }
    );

    setErrors(nextErrors);
    return nextErrors;
  }

  function updateImage(file: File) {
    if (!acceptedImageTypes.includes(file.type)) {
      setImageError("Envie uma imagem nos formatos PNG, JPG ou JPEG.");
      return false;
    }

    if (file.size > maxImageSizeInBytes) {
      setImageError("A imagem deve ter no máximo 5MB.");
      return false;
    }

    revokeCurrentObjectUrl();

    const previewUrl = URL.createObjectURL(file);
    objectUrlRef.current = previewUrl;
    setImageError("");
    setForm((currentForm) => ({
      ...currentForm,
      imageName: file.name,
      imagePreviewUrl: previewUrl,
    }));

    return true;
  }

  function removeImage() {
    revokeCurrentObjectUrl();
    setImageError("");
    setForm((currentForm) => ({
      ...currentForm,
      imageName: "",
      imagePreviewUrl: "",
    }));
  }

  function removeCollectionPoint(pointId: number) {
    setCollectionPoints((currentPoints) =>
      currentPoints.filter((point) => point.id !== pointId)
    );
  }

  function resetForm() {
    revokeCurrentObjectUrl();
    setForm(initialAddWaterBodieForm);
    setErrors({});
    setImageError("");
    setCollectionPoints(initialCollectionPoints);
  }

  function revokeCurrentObjectUrl() {
    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
      objectUrlRef.current = null;
    }
  }

  return {
    form,
    errors,
    imageError,
    collectionPoints,
    updateField,
    validateForm,
    updateImage,
    removeImage,
    removeCollectionPoint,
    resetForm,
  };
}
