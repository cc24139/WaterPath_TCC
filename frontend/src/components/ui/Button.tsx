"use client";
import {Inter} from "next/font/google";
import React from "react";




type ButtonProps = {
  children: React.ReactNode;
  onClick: () => void;
  className? : string;
};

export function ButtonComponent({ children, onClick, className }: ButtonProps) {
  return (
    <button
      onClick={onClick}
      className={className}
    >
      {children}
    </button>
  );
}

export default ButtonComponent;
