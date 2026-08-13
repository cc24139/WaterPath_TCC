import { ImageUploadCard } from "@/components/ui/ImageUploadCard";

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
  return (
    <ImageUploadCard
      inputId="analysis-image-upload"
      title="Imagem da análise"
      imageName={imageName}
      imagePreviewUrl={imagePreviewUrl}
      onImageChange={onImageChange}
      onRemoveImage={onRemoveImage}
    />
  );
}
