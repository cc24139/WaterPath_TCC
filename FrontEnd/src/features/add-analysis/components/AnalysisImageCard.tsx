import { ImageUploadCard } from "@/components/ui/ImageUploadCard";

interface AnalysisImageCardProps {
  imageName: string;
  error?: string;
  imagePreviewUrl: string;
  onImageChange: (file: File) => boolean;
  onRemoveImage: () => void;
}

export function AnalysisImageCard({
  error,
  imageName,
  imagePreviewUrl,
  onImageChange,
  onRemoveImage,
}: AnalysisImageCardProps) {
  return (
    <ImageUploadCard
      inputId="add-analysis-image"
      error={error}
      description="Anexe uma foto da coleta para a análise integrada."
      title="Imagem da análise"
      imageName={imageName}
      imagePreviewUrl={imagePreviewUrl}
      onImageChange={onImageChange}
      onRemoveImage={onRemoveImage}
    />
  );
}
