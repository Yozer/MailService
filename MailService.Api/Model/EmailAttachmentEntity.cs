namespace MailService.Api.Model
{
    public class EmailAttachmentEntity
    {
        public string Name { get; private set; }
        public byte[] Data { get; private set; }

        public EmailAttachmentEntity(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }
    }
}