using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketEvidenceDTO
    {
        [Required(ErrorMessage = "Please describe the evidence")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public List<EvidenceFileDTO> Files { get; set; } = new();
    }

    public class EvidenceFileDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }
}
