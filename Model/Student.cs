namespace Model
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int ZipCode { get; set; }
        public string PostalDistrict { get; set; }
        public string Email { get; set; }

        public override bool Equals(object obj)
        {
            if (! (obj is Student))
            {
                return false;
            }
            var student = (Student) obj;
            return (this.Id == student.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
