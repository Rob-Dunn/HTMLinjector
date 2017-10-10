using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTMLinjectorServices;

namespace HTMLinjector_NUnit
{
    public class FolderMock : IFolder
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Task CopyFileHere(IFile file)
        {
            throw new NotImplementedException();
        }

        public Task<IFolder> CreateChildFolder(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IFile> CreateFileAsync(string name, string contents)
        {
            throw new NotImplementedException();
        }

        public Task Delete()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Exists(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IFolder> GetChildFolderAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<IFolder>> GetChildFoldersAsync()
        {
            List<IFolder> folders = new List<IFolder>();

            switch (this.Path)
            {
                case "Folder1":
                    folders.Add(new FolderMock() { Name = "Folder1Folder0", Path = "Folder1/Folder0" });
                    break;
            }

            return folders;
        }

        public async Task<List<IFile>> GetFilesAsync()
        {
            List<IFile> files = new List<IFile>();

            switch (this.Path)
            {
                case "Folder0":
                    files.Add(new FileMock() { Name = "Folder0File0.notatemplate", Path = this.Path });
                    files.Add(new FileMock() { Name = "Folder0File1.template", Path = this.Path });
                    break;
            }

            return files;
        }
    }
}
