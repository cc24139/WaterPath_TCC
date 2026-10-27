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
      description="Opcional: anexe uma foto. Sem arquivo, a API procura a imagem mais recente salva para este corpo hídrico."
      title="Imagem da análise"
      imageName={imageName}
      imagePreviewUrl={imagePreviewUrl}
      onImageChange={onImageChange}
      onRemoveImage={onRemoveImage}
    />
  );
}
