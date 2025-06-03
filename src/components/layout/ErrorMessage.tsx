import React from 'react';

interface ErrorMessageProps {
  message: string;
  className?: string;
}

export function ErrorMessage({ message, className = '' }: ErrorMessageProps) {
  if (!message) return null;
  
  return (
    <div className={`p-4 bg-red-50 text-red-700 rounded-md ${className}`}>
      {message}
    </div>
  );
} 