export interface AddWaterBodieFormValues {
  name: string;
  location: string;
  size: string;
  ehPrivado: boolean;
}

export type AddWaterBodieFormState = AddWaterBodieFormValues;

export type AddWaterBodieFieldName = "name" | "location" | "size";
export type RequiredWaterBodieField = "name" | "location" | "size";
export type AddWaterBodieErrors = Partial<
  Record<RequiredWaterBodieField, string>
>;
