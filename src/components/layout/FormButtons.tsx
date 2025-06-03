import React from 'react';

interface FormButtonsProps {
  onBack: () => void;
  onSubmit: () => void;
  submitText?: string;
  submitDisabled?: boolean;
  isLoading?: boolean;
  className?: string;
}

export function FormButtons({ 
  onBack, 
  onSubmit, 
  submitText = 'Salvesta', 
  submitDisabled = false,
  isLoading = false,
  className = ''
}: FormButtonsProps) {
  return (
    <div className={`flex justify-center space-x-4 pt-4 ${className}`}>
      <button 
        type="button" 
        onClick={onBack}
        className="px-6 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
        disabled={isLoading}
      >
        Tagasi
      </button>
      <button 
        type="submit" 
        onClick={onSubmit}
        className="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300"
        disabled={submitDisabled || isLoading}
      >
        {isLoading ? 'Salvestan...' : submitText}
      </button>
    </div>
  );
} 