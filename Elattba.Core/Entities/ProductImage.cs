namespace ElAtaba.Domain.Entities;

/// <summary>Multiple images per product - encourages sellers to add more for buyer trust.</summary>
public class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>The main thumbnail shown in listings.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// The image's visual "fingerprint" for the Image Search feature - a JSON-serialized array
    /// of floats produced by the ONNX embedding model. Null until the image has been processed.
    /// Nullable/optional by design: existing rows before this feature shipped simply won't be
    /// searchable by image until reprocessed, without breaking anything else.
    /// </summary>
    public string? EmbeddingVector { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

