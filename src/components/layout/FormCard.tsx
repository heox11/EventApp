import React from 'react';

interface FormCardProps {
  children: React.ReactNode;
  className?: string;
}

export function FormCard({ children, className = '' }: FormCardProps) {
  return (
    <div className={`bg-white shadow-md rounded-lg overflow-hidden ${className}`}>
      <div className="px-6 py-5">
        {children}
      </div>
    </div>
  );
} 