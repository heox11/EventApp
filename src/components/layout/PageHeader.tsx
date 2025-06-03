import React from 'react';

interface PageHeaderProps {
  title: string;
  imageSrc?: string;
  imageAlt?: string;
}

export function PageHeader({ title, imageSrc = "/libled.jpg", imageAlt = "Grass with dew" }: PageHeaderProps) {
  return (
    <div className="max-w-8xl mx-auto grid grid-cols-1 md:grid-cols-2 items-stretch">
      <div className="bg-blue-900 text-white px-6 sm:px-8 lg:px-12 py-8 flex items-center">
        <h1 className="text-3xl font-bold">{title}</h1>
      </div>
      <div className="md:h-[100px] h-40">
        <img src={imageSrc} alt={imageAlt} className="w-full h-full object-cover" />
      </div>
    </div>
  );
} 