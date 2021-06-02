using System;
using System.Collections.Generic;
using System.Text;

namespace DICOMFileSeeker
{
    class DicomDataElement
    {
        public DicomTag Tag { get; private set; }
        public string ValueField { get; private set; } // The value Field is more like a union..or a class that can contain instances of the DICOM types (CS, UI, etc).

        DicomDataElement(DicomTag tag, string valueField)
        {
            this.Tag = tag;
            this.ValueField = valueField;
        }
    }
}
