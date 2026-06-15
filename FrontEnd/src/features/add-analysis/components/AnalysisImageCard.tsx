"use client";

import type { ChangeEvent, DragEvent } from "react";
import { LuImage, LuUpload, LuX } from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";

interface AnalysisImageCardProps {
  imageName: string;
  imagePreviewUrl: string;
  onImageChange: (file: File) => boolean;
  onRemoveImage: () => void;
}

export function AnalysisImageCard({
  imageName,
  imagePreviewUrl,
  onImageChange,
  onRemoveImage,
}: AnalysisImageCardProps) {
  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (file) {
      onImageChange(file);
    }

    event.target.value = "";
  }

  function handleDrop(event: DragEvent<HTMLLabelElement>) {
    event.preventDefault();

    const file = event.dataTransfer.files?.[0];

    if (file) {
      onImageChange(file);
    }
  }

  return (
    <Card className="!rounded-lg !px-5 !py-5 lg:!px-6">
      <CardHeader className="mb-5 flex items-center gap-3">
        <LuImage className="h-6 w-6 text-primary" />
        <CardTitle className="text-[18px] sm:text-[19px]">
          Imagem da análise
        </CardTitle>
      </CardHeader>

      <CardContent className="flex flex-col gap-4">
        <label
          htmlFor="analysis-image-upload"
          onDragOver={(event) => event.preventDefault()}
          onDrop={handleDrop}
          className="flex min-h-36 cursor-pointer flex-col items-center justify-center rounded-lg border border-dashed border-text-secondary/45 px-5 py-6 text-center transition-colors hover:border-primary hover:bg-secondary/10"
        >
          <input
            id="analysis-image-upload"
            type="file"
            accept="image/png,image/jpeg,image/jpg"
            onChange={handleFileChange}
            className="sr-only"
          />

          <LuUpload className="h-9 w-9 text-text-secondary" />
          <span className="mt-3 font-heading text-[12px] font-semibold text-text-primary">
            Arraste uma imagem ou clique para enviar
          </span>
          <span className="mt-1 font-heading text-[11px] font-medium text-text-secondary">
            PNG, JPG ou JPEG até 5MB
          </span>
        </label>

        <div
          role="img"
          aria-label={
            imageName
              ? `Prévia da imagem ${imageName}`
              : "Prévia de rio para a análise"
          }
          className="relative aspect-[16/6] min-h-28 overflow-hidden rounded-lg bg-cover bg-center shadow-[0_5px_16px_rgba(0,0,0,0.12)]"
          style={{ backgroundImage: `url("${imagePreviewUrl}")` }}
        >
          {imageName ? (
            <button
              type="button"
              onClick={onRemoveImage}
              aria-label="Remover imagem"
              className="absolute right-2 top-2 inline-flex h-7 w-7 items-center justify-center rounded-full bg-white text-text-primary shadow-default transition-colors hover:text-primary"
            >
              <LuX className="h-4 w-4" />
            </button>
          ) : null}
        </div>
      </CardContent>
    </Card>
  );
}
