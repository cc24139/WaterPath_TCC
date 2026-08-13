"use client";

import Image from "next/image";
import { useState, type ChangeEvent, type DragEvent } from "react";
import { LuImage, LuUpload, LuX } from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";

interface ImageUploadCardProps {
  inputId: string;
  title: string;
  description?: string;
  imageName: string;
  imagePreviewUrl: string;
  onImageChange: (file: File) => void;
  onRemoveImage: () => void;
  error?: string;
}

export function ImageUploadCard({
  inputId,
  title,
  description,
  imageName,
  imagePreviewUrl,
  onImageChange,
  onRemoveImage,
  error,
}: ImageUploadCardProps) {
  const [isDragging, setIsDragging] = useState(false);
  const errorId = error ? `${inputId}-error` : undefined;

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (file) {
      onImageChange(file);
    }

    event.target.value = "";
  }

  function handleDrop(event: DragEvent<HTMLLabelElement>) {
    event.preventDefault();
    setIsDragging(false);

    const file = event.dataTransfer.files?.[0];

    if (file) {
      onImageChange(file);
    }
  }

  return (
    <Card className="!rounded-lg !px-5 !py-5 lg:!px-6">
      <CardHeader className="mb-5 flex items-start gap-3">
        <LuImage className="mt-0.5 h-6 w-6 shrink-0 text-primary" />
        <span className="min-w-0">
          <CardTitle className="text-[18px] sm:text-[19px]">
            {title}
          </CardTitle>
          {description ? (
            <p className="mt-2 font-heading text-[12px] font-medium leading-relaxed text-text-secondary">
              {description}
            </p>
          ) : null}
        </span>
      </CardHeader>

      <CardContent className="flex flex-col gap-4">
        <label
          htmlFor={inputId}
          onDragEnter={() => setIsDragging(true)}
          onDragLeave={() => setIsDragging(false)}
          onDragOver={(event) => event.preventDefault()}
          onDrop={handleDrop}
          className={`flex min-h-36 cursor-pointer flex-col items-center justify-center rounded-lg border border-dashed px-5 py-6 text-center transition-colors ${
            isDragging
              ? "border-primary bg-secondary/10"
              : "border-text-secondary/45 hover:border-primary hover:bg-secondary/10"
          }`}
        >
          <input
            id={inputId}
            type="file"
            accept="image/png,image/jpeg,image/jpg"
            onChange={handleFileChange}
            aria-describedby={errorId}
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

        {error ? (
          <p
            id={errorId}
            role="alert"
            className="font-heading text-[11px] font-semibold text-contrast"
          >
            {error}
          </p>
        ) : null}

        {imagePreviewUrl ? (
          <div className="relative aspect-[16/7] min-h-28 overflow-hidden rounded-lg bg-placeholder shadow-[0_5px_16px_rgba(0,0,0,0.12)]">
            <Image
              src={imagePreviewUrl}
              alt={
                imageName
                  ? `Prévia da imagem ${imageName}`
                  : `Prévia para ${title.toLocaleLowerCase("pt-BR")}`
              }
              fill
              unoptimized
              sizes="(min-width: 1280px) 390px, 100vw"
              className="object-cover"
            />

            {imageName ? (
              <button
                type="button"
                onClick={onRemoveImage}
                aria-label="Remover imagem"
                className="absolute right-2 top-2 inline-flex h-8 w-8 items-center justify-center rounded-full bg-white text-text-primary shadow-default transition-colors hover:text-contrast focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <LuX className="h-4 w-4" />
              </button>
            ) : null}
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
