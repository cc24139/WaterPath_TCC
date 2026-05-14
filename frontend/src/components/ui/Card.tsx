import type { ComponentPropsWithoutRef } from "react";

type CardProps = ComponentPropsWithoutRef<"section">;
type CardHeaderProps = ComponentPropsWithoutRef<"div">;
type CardTitleProps = ComponentPropsWithoutRef<"h3">;
type CardContentProps = ComponentPropsWithoutRef<"div">;
type CardFooterProps = ComponentPropsWithoutRef<"div">;

export function Card({ className = "", children, ...props }: CardProps) {
  return (
    <section
      className={`bg-white rounded-xl p-4 shadow-default sm:rounded-2xl ${className}`}
      {...props}
    >
      {children}
    </section>
  );
}

export function CardHeader({
  className = "",
  children,
  ...props
}: CardHeaderProps) {
  return (
    <div className={`mb-4 ${className}`} {...props}>
      {children}
    </div>
  );
}

export function CardTitle({
  className = "",
  children,
  ...props
}: CardTitleProps) {
  return (
    <h3
      className={`font-heading text-[16px] font-bold text-text-primary ${className}`}
      {...props}
    >
      {children}
    </h3>
  );
}

export function CardContent({
  className = "",
  children,
  ...props
}: CardContentProps) {
  return (
    <div className={className} {...props}>
      {children}
    </div>
  );
}

export function CardFooter({
  className = "",
  children,
  ...props
}: CardFooterProps) {
  return (
    <div className={`mt-4 ${className}`} {...props}>
      {children}
    </div>
  );
}
