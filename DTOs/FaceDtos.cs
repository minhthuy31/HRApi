namespace HRApi.DTOs
{
    public class RegisterFaceDto
    {
        public string MaNhanVien { get; set; }
        public float[] FaceDescriptor { get; set; }
    }

    public class CheckInFaceDto
    {
        public float[] FaceDescriptor { get; set; }
    }
}