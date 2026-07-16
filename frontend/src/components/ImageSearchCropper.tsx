import React, { useState, useRef } from 'react';
import { Upload, Image as ImageIcon, X } from 'lucide-react';

interface ImageSearchCropperProps {
  onImageSelected: (file: File) => void;
  onClear: () => void;
  isLoading: boolean;
}

const ImageSearchCropper: React.FC<ImageSearchCropperProps> = ({ onImageSelected, onClear, isLoading }) => {
  const [dragActive, setDragActive] = useState(false);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  };

  const processFile = (file: File) => {
    if (!file.type.startsWith('image/')) {
      alert('Please upload an image file (PNG, JPG, JPEG).');
      return;
    }
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result as string);
    };
    reader.readAsDataURL(file);
    onImageSelected(file);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      processFile(e.dataTransfer.files[0]);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]) {
      processFile(e.target.files[0]);
    }
  };

  const handleClear = () => {
    setImagePreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    onClear();
  };

  const onButtonClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className="card" style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'center', padding: '2rem' }}>
      <input
        ref={fileInputRef}
        type="file"
        style={{ display: 'none' }}
        onChange={handleChange}
        accept="image/*"
      />

      {!imagePreview ? (
        <div
          style={{
            border: dragActive ? '2px dashed var(--primary)' : '2px dashed var(--border-color)',
            borderRadius: 'var(--radius-lg)',
            padding: '3rem 2rem',
            backgroundColor: dragActive ? 'rgba(var(--primary-rgb), 0.05)' : 'var(--bg-main)',
            cursor: 'pointer',
            transition: 'all 0.2s',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: '1rem'
          }}
          onDragEnter={handleDrag}
          onDragOver={handleDrag}
          onDragLeave={handleDrag}
          onDrop={handleDrop}
          onClick={onButtonClick}
        >
          <div style={{
            background: 'rgba(255, 183, 3, 0.1)',
            padding: '1rem',
            borderRadius: '50%',
            color: 'var(--primary-hover)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <Upload size={32} />
          </div>
          <div>
            <h3 style={{ fontSize: '1.2rem', marginBottom: '0.5rem' }}>Drag & Drop Image here</h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
              or click to browse from your device
            </p>
          </div>
          <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
            Supports PNG, JPG, JPEG. Recommended square aspect ratio.
          </span>
        </div>
      ) : (
        <div style={{ position: 'relative', display: 'inline-block', width: '100%', maxWidth: '350px' }}>
          <img
            src={imagePreview}
            alt="Search preview"
            style={{
              width: '100%',
              borderRadius: 'var(--radius-md)',
              border: '2px solid var(--border-color)',
              boxShadow: 'var(--shadow-md)',
              maxHeight: '350px',
              objectFit: 'contain'
            }}
          />
          <button
            onClick={handleClear}
            style={{
              position: 'absolute',
              top: '-10px',
              right: '-10px',
              background: 'var(--color-danger)',
              color: 'white',
              border: 'none',
              borderRadius: '50%',
              width: '28px',
              height: '28px',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: 'var(--shadow-md)'
            }}
            title="Remove Image"
            disabled={isLoading}
          >
            <X size={16} />
          </button>
        </div>
      )}

      {imagePreview && (
        <div style={{ marginTop: '1.5rem', display: 'flex', justifyContent: 'center', gap: '1rem' }}>
          <button 
            className="btn btn-outline" 
            onClick={handleClear}
            disabled={isLoading}
          >
            Choose Another
          </button>
        </div>
      )}
    </div>
  );
};

export default ImageSearchCropper;
