import type { FormFieldOption } from "@/components/ui/FormField";
import type { AddWaterBodieFormState } from "@/features/add-water-bodie/types/addWaterBodie";

export const initialAddWaterBodieForm: AddWaterBodieFormState = {
  name: "",
  location: "",
  size: "",
  ehPrivado: false,
};

export const waterBodyAccessOptions: FormFieldOption[] = [
  { label: "Acesso público", value: "false" },
  { label: "Área privada", value: "true" },
];
