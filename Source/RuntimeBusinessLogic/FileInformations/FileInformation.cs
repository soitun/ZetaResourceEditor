﻿namespace ZetaResourceEditor.RuntimeBusinessLogic.FileInformations
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using DL;
    using FileGroups;
    using Projects;
    using Zeta.VoyagerLibrary.Common;
    using ZetaLongPaths;

    [DebuggerDisplay(@"File = {File.Name}")]
    public class FileInformation :
        ITranslationStateInformation,
        IComparable,
        IComparable<FileInformation>,
        IUniqueID
    {
        private Guid _uniqueID;

        public FileInformation(
            FileGroup fileGroup)
        {
            FileGroup = fileGroup;
        }

        public FileGroup FileGroup { get; }

        public FileGroupStateColor TranslationStateColor => FileGroup.TranslationStateColor;

        public ZlpFileInfo File { get; set; }

        public int CompareTo(object obj)
        {
            return CompareTo((FileInformation)obj);
        }

        public int CompareTo(FileInformation other)
        {
            var x = ZlpPathHelper.GetFileNameWithoutExtension(File.FullName);
            var y = ZlpPathHelper.GetFileNameWithoutExtension(other.File.FullName);

            if ((x.Contains(@".") && y.Contains(@".")) ||
                (!x.Contains(@".") && !y.Contains(@".")))
            {
                return string.CompareOrdinal(x, y);
            }
            else
            {
                if (x.Contains(@"."))
                {
                    if (x.StartsWith(y, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return +1;
                    }
                    else
                    {
                        return string.CompareOrdinal(x, y);
                    }
                }
                else
                {
                    if (y.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return -1;
                    }
                    else
                    {
                        return string.CompareOrdinal(x, y);
                    }
                }
            }
        }

        public void StoreToXml(Project project, XmlElement parentNode)
        {
            if (parentNode.OwnerDocument != null)
            {
                var a = parentNode.OwnerDocument.CreateAttribute(@"filePath");
                a.Value = project.MakeRelativeFilePath(File.FullName);
                parentNode.Attributes.Append(a);

                a = parentNode.OwnerDocument.CreateAttribute(@"absoluteFilePath");
                a.Value = File.FullName;
                parentNode.Attributes.Append(a);

                a = parentNode.OwnerDocument.CreateAttribute(@"uniqueID");
                a.Value = _uniqueID.ToString();
                parentNode.Attributes.Append(a);
            }
        }

        public bool LoadFromXml(Project project, XmlNode parentNode)
        {
            if (parentNode.Attributes != null)
            {
                XmlHelper.ReadAttribute(
                    out _uniqueID,
                    parentNode.Attributes[@"uniqueID"]);
            }

            if (_uniqueID == Guid.Empty)
            {
                _uniqueID = Guid.NewGuid();
            }

            // --

            string filePath = null;
            if (parentNode.Attributes != null)
            {
                XmlHelper.ReadAttribute(
                    out filePath,
                    parentNode.Attributes[@"filePath"]);
            }

            filePath = project.MakeAbsoluteFilePath(filePath);

            if (!string.IsNullOrEmpty(filePath) && ZlpIOHelper.FileExists(filePath))
            {
                File = new ZlpFileInfo(filePath);
                return true;
            }
            else
            {
                // Try absolute file path if relative one fails.
                filePath = null;
                if (parentNode.Attributes != null)
                {
                    XmlHelper.ReadAttribute(
                        out filePath,
                        parentNode.Attributes[@"absoluteFilePath"]);
                }

                if (!string.IsNullOrEmpty(filePath) && ZlpIOHelper.FileExists(filePath))
                {
                    File = new ZlpFileInfo(filePath);
                    return true;
                }
                else
                {
                    // Try in same folder as project file, if both above failed.

                    filePath = null;
                    if (parentNode.Attributes != null)
                    {
                        XmlHelper.ReadAttribute(
                            out filePath,
                            parentNode.Attributes[@"filePath"]);
                    }
                    if (string.IsNullOrEmpty(filePath) && parentNode.Attributes != null)
                    {
                        XmlHelper.ReadAttribute(
                            out filePath,
                            parentNode.Attributes[@"absoluteFilePath"]);
                    }

                    if (string.IsNullOrEmpty(filePath))
                    {
                        File = null;
                        return false;
                    }
                    else
                    {
                        filePath = ZlpPathHelper.GetFileNameFromFilePath(filePath);
                        filePath = project.MakeAbsoluteFilePath(filePath);

                        if (!string.IsNullOrEmpty(filePath) && ZlpIOHelper.FileExists(filePath))
                        {
                            File = new ZlpFileInfo(filePath);
                            return true;
                        }
                        else
                        {
                            File = null;
                            return false;
                        }
                    }
                }
            }
        }

        public Guid UniqueID => _uniqueID;
    }
}