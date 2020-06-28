using System;
using System.Collections.Generic;
using System.Text;

namespace DICOMFileSeeker
{
    public class DicomTag
    {
        public Int32 Group { get; set; }
        public Int32 Element { get; set; }

        public DicomTag(Int32 group, Int32 element)
        {
            this.Group = group;
            this.Element = element;
        }
    }
}