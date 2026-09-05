import type { FormEvent, InputHTMLAttributes, ReactNode } from "react";

import { Button } from "./Button";
import { InputField } from "./InputField";

type AuthField = InputHTMLAttributes<HTMLInputElement> & {
  placeholder: string;
};

interface AuthFormCardProps {
  title: ReactNode;
  subtitle: string;
  fields: AuthField[];
  primaryActionText: string;
  secondaryActionText?: string;
  recoveryLink?: ReactNode;
  onPrimaryAction?: () => void;
  onSecondaryAction?: () => void;
  isSubmitting?: boolean;
  error?: string | null;
  successMessage?: string | null;
}

export function AuthFormCard({
  title,
  subtitle,
  fields,
  primaryActionText,
  secondaryActionText = "Cancelar",
  recoveryLink,
  onPrimaryAction,
  onSecondaryAction,
  isSubmitting = false,
  error,
  successMessage,
}: AuthFormCardProps) {
  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!isSubmitting) {
      onPrimaryAction?.();
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      aria-busy={isSubmitting}
      className="flex w-full max-w-[560px] flex-col items-center justify-center rounded-lg bg-white px-5 py-7 shadow-md sm:px-8 md:px-7 md:py-8 lg:px-10"
    >
      <h1 className="px-2 pb-3 text-[22px] font-semibold leading-tight text-text-primary sm:text-2xl md:text-[23px] lg:text-2xl">
        {title}
      </h1>

      <h2 className="pb-4 text-[14px] font-bold text-text-secondary sm:text-base md:pb-5">
        {subtitle}
      </h2>

      <div className="flex w-full flex-col items-center gap-3 md:gap-3.5">
        {fields.map((field) => (
          <InputField key={`${field.name ?? field.placeholder}`} {...field} />
        ))}
      </div>

      {recoveryLink ? (
        <p className="mt-4 text-[13px] font-medium text-placeholder sm:text-sm">
          {recoveryLink}
        </p>
      ) : null}

      {error && (
        <p role="alert" className="mt-3 w-full text-sm text-red-600">
          {error}
        </p>
      )}

      {successMessage && !error && (
        <p role="status" className="mt-3 w-full text-sm text-green-700">
          {successMessage}
        </p>
      )}

      <div className="mt-5 flex w-full flex-wrap items-center justify-center gap-3 sm:gap-4">
        <Button
          type="button"
          onClick={onSecondaryAction}
          text={secondaryActionText}
          variant="secondary"
          disabled={isSubmitting}
        />
        <Button
          type="submit"
          text={primaryActionText}
          variant="primary"
          disabled={isSubmitting}
        />
      </div>
    </form>
  );
}
