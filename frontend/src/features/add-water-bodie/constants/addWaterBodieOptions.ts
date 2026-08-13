import type { FormFieldOption } from "@/components/ui/FormField";
import type {
  AddWaterBodieFormState,
  CollectionPoint,
} from "@/features/add-water-bodie/types/addWaterBodie";

export const initialAddWaterBodieForm: AddWaterBodieFormState = {
  name: "",
  type: "",
  city: "",
  state: "",
  location: "",
  basin: "",
  description: "",
  imageName: "",
  imagePreviewUrl: "",
};

export const waterBodyTypeOptions: FormFieldOption[] = [
  { label: "Rio", value: "Rio" },
  { label: "Lago", value: "Lago" },
  { label: "Lagoa", value: "Lagoa" },
  { label: "Córrego", value: "Córrego" },
  { label: "Represa", value: "Represa" },
  { label: "Canal", value: "Canal" },
  { label: "Outro", value: "Outro" },
];

export const brazilianStateOptions: FormFieldOption[] = [
  { label: "Acre (AC)", value: "AC" },
  { label: "Alagoas (AL)", value: "AL" },
  { label: "Amapá (AP)", value: "AP" },
  { label: "Amazonas (AM)", value: "AM" },
  { label: "Bahia (BA)", value: "BA" },
  { label: "Ceará (CE)", value: "CE" },
  { label: "Distrito Federal (DF)", value: "DF" },
  { label: "Espírito Santo (ES)", value: "ES" },
  { label: "Goiás (GO)", value: "GO" },
  { label: "Maranhão (MA)", value: "MA" },
  { label: "Mato Grosso (MT)", value: "MT" },
  { label: "Mato Grosso do Sul (MS)", value: "MS" },
  { label: "Minas Gerais (MG)", value: "MG" },
  { label: "Pará (PA)", value: "PA" },
  { label: "Paraíba (PB)", value: "PB" },
  { label: "Paraná (PR)", value: "PR" },
  { label: "Pernambuco (PE)", value: "PE" },
  { label: "Piauí (PI)", value: "PI" },
  { label: "Rio de Janeiro (RJ)", value: "RJ" },
  { label: "Rio Grande do Norte (RN)", value: "RN" },
  { label: "Rio Grande do Sul (RS)", value: "RS" },
  { label: "Rondônia (RO)", value: "RO" },
  { label: "Roraima (RR)", value: "RR" },
  { label: "Santa Catarina (SC)", value: "SC" },
  { label: "São Paulo (SP)", value: "SP" },
  { label: "Sergipe (SE)", value: "SE" },
  { label: "Tocantins (TO)", value: "TO" },
];

export const initialCollectionPoints: CollectionPoint[] = [
  {
    id: 1,
    name: "Ponto de coleta 01",
    reference: "Próximo à Ponte Rio-Niterói, Icaraí",
    coordinates: "22.9186° S, 43.1165° W",
    type: "Montante",
  },
  {
    id: 2,
    name: "Ponto de coleta 02",
    reference: "Foz do Rio Icaraí, Baía de Guanabara",
    coordinates: "22.8752° S, 43.1421° W",
    type: "Jusante",
  },
];
