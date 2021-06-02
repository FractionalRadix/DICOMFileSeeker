using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DICOMFileSeeker
{
    class DicomFileParser
    {
        DicomFileParser(string filename)
        {
            //TODO!~ FIRST check if there is a preamble - a number of 0's followed by DICM.
            // See http://justsolve.archiveteam.org/wiki/DICOM#Types_of_DICOM_files for the different ways DICOM files may work.

            ISet<DicomDataElement> dataElements = new HashSet<DicomDataElement>();
            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                //TODO!+ Read the data elements.
                Span<byte> buffer = new Span<byte>(new byte[4]);
                int res = fileStream.Read(buffer);
                uint buf0 = ((uint)buffer[0]) << 8;
                uint buf1 = buffer[1];
                uint groupId = buf0 | buf1;
                uint buf2 = ((uint)buffer[2]) << 8;
                uint buf3 = buffer[3];
                uint elementId = buf2 | buf3;

                //TODO!+

            }

        }
    }
}
