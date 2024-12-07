using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.DTO
{
    public class Settings
    {
        public string? Process { get; set; }
        public string? Executable { get; set; }
        public string? Path { get; set; }
        public int LoopTime { get; set; }

        public bool IsRunning { get; set; }
    }
}
