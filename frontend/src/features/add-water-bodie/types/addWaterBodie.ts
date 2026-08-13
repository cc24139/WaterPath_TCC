export interface AddWaterBodieFormValues {
  name: string;
  type: string;
  city: string;
  state: string;
  location: string;
  basin: string;
  description: string;
}

export interface AddWaterBodieFormState extends AddWaterBodieFormValues {
  imageName: string;
  imagePreviewUrl: string;
}

export type AddWaterBodieFieldName = keyof AddWaterBodieFormValues;
export type RequiredWaterBodieField = "name" | "type" | "city" | "state";
export type AddWaterBodieErrors = Partial<
  Record<RequiredWaterBodieField, string>
>;

export type CollectionPointType =
  | "Montante"
  | "Jusante"
  | "Ponto principal";

export interface CollectionPoint {
  id: number;
  name: string;
  reference: string;
  coordinates?: string;
  type: CollectionPointType;
}
