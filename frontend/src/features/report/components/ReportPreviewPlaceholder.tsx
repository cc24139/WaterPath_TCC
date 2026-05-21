import { LuFileText } from "react-icons/lu";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";

export function ReportPreviewPlaceholder() {
  return (
    <Card className="flex min-h-[420px] w-full min-w-0 flex-col !px-5 !py-5 sm:min-h-[560px] sm:!px-6 xl:min-h-[690px]">
      <CardHeader className="mb-8">
        <CardTitle className="text-[16px] sm:text-[17px]">
          Preview do relatório
        </CardTitle>
        <p className="mt-1 font-heading text-[11px] font-medium text-[#767676] sm:text-[12px]">
          O preview será exibido aqui futuramente.
        </p>
      </CardHeader>

      <CardContent className="flex flex-1 items-center justify-center">
        <div className="flex min-h-[260px] w-full max-w-[620px] flex-col items-center justify-center rounded-xl border border-dashed border-placeholder bg-[#F7F7F7] px-6 text-center sm:min-h-[340px] xl:min-h-[430px]">
          <LuFileText className="h-12 w-12 text-primary" />
          <h2 className="mt-4 font-heading text-[16px] font-bold text-text-primary">
            Preview do relatório
          </h2>
          <p className="mt-2 max-w-[310px] font-heading text-[12px] font-medium leading-relaxed text-[#767676]">
            Configure as opções ao lado para gerar uma prévia visual nesta área.
          </p>
        </div>
      </CardContent>
    </Card>
  );
}
