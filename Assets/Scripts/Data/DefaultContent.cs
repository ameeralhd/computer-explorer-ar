using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Core;
using UnityEngine;
using Ids = ComputerExplorer.Core.AppConstants.HardwareIds;

namespace ComputerExplorer.Data
{
    /// <summary>
    /// Seed educational content in Bahasa Indonesia and English. Used directly when no content assets exist,
    /// and by "Computer Explorer > Create Content Assets" to generate editable ScriptableObjects in
    /// Assets/ScriptableObjects. After generating assets, edit the assets — not this file.
    /// Every text is written as a pair: ("Bahasa Indonesia", "English").
    /// Hotspot positions are in model units and match HardwareModelFactory.
    /// </summary>
    public static class DefaultContent
    {
        public class Bundle
        {
            public List<HardwareData> Hardware = new List<HardwareData>();
            public List<ModuleData> Modules = new List<ModuleData>();
            public List<ScenarioData> Scenarios = new List<ScenarioData>();
            public List<QuestionData> Questions = new List<QuestionData>();
        }

        public static Bundle Build()
        {
            var b = new Bundle();

            // ------------------------------------------------------------------ Hardware
            var cpu = Hw(b, Ids.Cpu, "CPU", ("Central Processing Unit", "Central Processing Unit"), HardwareCategory.Processing, "cpu", "CPU_Target",
                ("Otak komputer yang menjalankan instruksi program.",
                 "The brain of the computer that runs program instructions."),
                ("CPU (Central Processing Unit) mengambil instruksi dari memori, menerjemahkannya, lalu mengeksekusinya. " +
                 "Di dalam CPU terdapat Control Unit (CU) yang mengatur urutan kerja, Arithmetic Logic Unit (ALU) yang menghitung " +
                 "dan membandingkan data, serta register sebagai tempat penyimpanan sangat cepat berukuran kecil. " +
                 "Siklus kerjanya disebut fetch – decode – execute.",
                 "The CPU (Central Processing Unit) fetches instructions from memory, decodes them and executes them. " +
                 "Inside the CPU are the Control Unit (CU), which directs the order of work, the Arithmetic Logic Unit (ALU), which " +
                 "calculates and compares data, and registers, which are tiny, very fast storage locations. " +
                 "This work cycle is called fetch – decode – execute."),
                ("Memproses instruksi dan data: menghitung, membandingkan, dan mengatur kerja komponen lain.",
                 "Processes instructions and data: it calculates, compares and coordinates the other components."),
                ("Saat kamu menekan tombol \"=\" di aplikasi kalkulator, ALU di dalam CPU yang menghitung hasilnya.",
                 "When you press \"=\" in a calculator app, the ALU inside the CPU works out the result."),
                Hs("alu", ("ALU (Arithmetic Logic Unit)", "ALU (Arithmetic Logic Unit)"),
                    ("ALU melakukan operasi aritmetika (tambah, kurang, kali, bagi) dan operasi logika (membandingkan, AND, OR). " +
                     "Contoh: menghitung nilai rata-rata rapor.",
                     "The ALU performs arithmetic (add, subtract, multiply, divide) and logic operations (compare, AND, OR). " +
                     "Example: calculating the average of your report card grades."), new Vector3(-0.17f, 0.2f, 0.12f)),
                Hs("control_unit", ("Control Unit (CU)", "Control Unit (CU)"),
                    ("Control Unit mengambil instruksi dari memori, menerjemahkannya, lalu memberi sinyal kepada ALU, memori, " +
                     "dan perangkat input/output. Ia seperti dirigen yang mengatur orkestra.",
                     "The Control Unit fetches instructions from memory, decodes them and sends signals to the ALU, memory and " +
                     "input/output devices. It is like a conductor leading an orchestra."), new Vector3(0.17f, 0.2f, 0.12f)),
                Hs("registers", ("Register & Cache", "Registers & Cache"),
                    ("Register dan cache adalah memori super cepat di dalam CPU. Data yang sedang diproses disimpan di sini " +
                     "agar CPU tidak perlu menunggu RAM.",
                     "Registers and cache are super-fast memory inside the CPU. Data being processed is kept here so the CPU " +
                     "does not have to wait for RAM."), new Vector3(0f, 0.2f, -0.16f)));

            var memory = Hw(b, Ids.Memory, "RAM", ("Random Access Memory", "Random Access Memory"), HardwareCategory.Memory, "memory-stick", "Memory_Target",
                ("Memori kerja sementara untuk program dan data yang sedang dipakai.",
                 "Temporary working memory for the programs and data in use."),
                ("RAM menyimpan program dan data yang sedang digunakan agar CPU dapat mengaksesnya dengan cepat. " +
                 "RAM bersifat volatile: isinya hilang ketika komputer dimatikan. Semakin besar kapasitas RAM, semakin banyak " +
                 "program yang dapat dibuka bersamaan tanpa melambat.",
                 "RAM holds the programs and data currently in use so the CPU can reach them quickly. " +
                 "RAM is volatile: its contents disappear when the computer is switched off. The more RAM a computer has, the " +
                 "more programs it can run at the same time without slowing down."),
                ("Menyimpan sementara instruksi dan data yang sedang diproses CPU.",
                 "Temporarily stores the instructions and data the CPU is working on."),
                ("Saat kamu membuka banyak tab browser sekaligus, setiap tab memakai ruang di RAM.",
                 "When you open many browser tabs at once, every tab takes up space in RAM."),
                Hs("chips", ("Chip Memori", "Memory Chips"),
                    ("Chip-chip hitam ini berisi jutaan sel memori kecil tempat data disimpan sementara dalam bentuk bit 0 dan 1.",
                     "These black chips contain millions of tiny memory cells where data is temporarily stored as bits (0 and 1)."),
                    new Vector3(-0.22f, 0.24f, 0.06f)),
                Hs("contacts", ("Pin Kontak Emas", "Gold Contact Pins"),
                    ("Pin kontak menghubungkan modul RAM ke slot di motherboard sehingga data dapat mengalir ke CPU.",
                     "The contact pins connect the RAM module to its motherboard slot so data can flow to the CPU."),
                    new Vector3(0.25f, 0.05f, 0.06f)),
                Hs("volatile", ("Sifat Volatile", "Volatile Memory"),
                    ("RAM hanya menyimpan data selama ada listrik. Karena itu, simpan pekerjaanmu ke storage sebelum mematikan komputer.",
                     "RAM only keeps data while it has power. That is why you should save your work to storage before switching off."),
                    new Vector3(0.3f, 0.3f, 0.06f)));

            var storage = Hw(b, Ids.Storage, "Storage", ("Penyimpanan (SSD / HDD)", "Storage (SSD / HDD)"), HardwareCategory.Storage, "hard-drive", "Storage_Target",
                ("Tempat menyimpan file dan program secara permanen.",
                 "Where files and programs are kept permanently."),
                ("Storage seperti SSD dan HDD menyimpan sistem operasi, aplikasi, dan file secara permanen (non-volatile), " +
                 "sehingga data tetap ada walaupun komputer dimatikan. SSD memakai chip flash sehingga lebih cepat daripada HDD " +
                 "yang memakai piringan magnetik berputar.",
                 "Storage such as SSDs and HDDs keeps the operating system, apps and files permanently (non-volatile), so data " +
                 "stays even when the computer is switched off. SSDs use flash chips, which makes them faster than HDDs, which " +
                 "use spinning magnetic disks."),
                ("Menyimpan data secara permanen dan memuatnya ke RAM ketika dibutuhkan.",
                 "Stores data permanently and loads it into RAM when needed."),
                ("Foto dan dokumen tugas sekolah yang kamu simpan tetap ada besok karena tersimpan di storage.",
                 "The photos and homework documents you save are still there tomorrow because they are kept in storage."),
                Hs("nand", ("Chip Flash (NAND)", "Flash Chips (NAND)"),
                    ("Chip flash menyimpan data tanpa memerlukan listrik, sehingga file tetap aman setelah komputer dimatikan.",
                     "Flash chips keep data without power, so your files stay safe after the computer is switched off."),
                    new Vector3(-0.2f, 0.1f, 0f)),
                Hs("controller", ("Controller", "Controller"),
                    ("Controller mengatur di mana data ditulis dan dibaca, serta menjaga agar chip flash awet.",
                     "The controller decides where data is written and read, and keeps the flash chips healthy."),
                    new Vector3(0.12f, 0.1f, 0f)),
                Hs("connector", ("Konektor", "Connector"),
                    ("Konektor menghubungkan storage ke motherboard. Data dari storage dimuat ke RAM sebelum diproses CPU.",
                     "The connector links the storage to the motherboard. Data from storage is loaded into RAM before the CPU processes it."),
                    new Vector3(0.46f, 0.08f, 0f)));

            var keyboard = Hw(b, Ids.Keyboard, "Keyboard", ("Papan Ketik", "Keyboard"), HardwareCategory.Input, "keyboard", "Keyboard_Target",
                ("Perangkat input untuk mengetik huruf, angka, dan perintah.",
                 "An input device for typing letters, numbers and commands."),
                ("Keyboard mengubah setiap tekanan tombol menjadi kode digital yang dikirim ke komputer. " +
                 "Kode ini masuk ke memori, lalu diproses CPU sehingga huruf muncul di layar.",
                 "A keyboard turns every key press into a digital code that is sent to the computer. " +
                 "The code goes into memory and is processed by the CPU, so the letter appears on the screen."),
                ("Memasukkan data teks dan perintah ke komputer.",
                 "Enters text and commands into the computer."),
                ("Mengetik nama dan alamat saat mengisi formulir pendaftaran online.",
                 "Typing your name and address when filling in an online registration form."),
                Hs("letters", ("Tombol Huruf & Angka", "Letter & Number Keys"),
                    ("Setiap tombol memiliki kode unik. Saat ditekan, kode dikirim ke komputer sebagai input.",
                     "Every key has a unique code. When pressed, the code is sent to the computer as input."),
                    new Vector3(-0.15f, 0.12f, 0.02f)),
                Hs("enter", ("Tombol Enter", "Enter Key"),
                    ("Tombol Enter mengirim perintah untuk menjalankan atau mengonfirmasi, misalnya mengirim pesan.",
                     "The Enter key sends a command to run or confirm something, such as sending a message."),
                    new Vector3(0.38f, 0.12f, 0f)),
                Hs("cable", ("Kabel / Koneksi", "Cable / Connection"),
                    ("Data dari keyboard dikirim melalui kabel USB atau secara nirkabel (Bluetooth) ke komputer.",
                     "Data from the keyboard travels to the computer through a USB cable or wirelessly (Bluetooth)."),
                    new Vector3(0f, 0.1f, -0.27f)));

            var mouse = Hw(b, Ids.Mouse, "Mouse", ("Tetikus", "Mouse"), HardwareCategory.Input, "mouse", "Mouse_Target",
                ("Perangkat input penunjuk untuk menggerakkan kursor.",
                 "A pointing input device that moves the cursor."),
                ("Mouse mendeteksi gerakan tangan melalui sensor optik dan mengirimkan posisi serta klik ke komputer.",
                 "A mouse detects hand movement with an optical sensor and sends the position and clicks to the computer."),
                ("Menggerakkan kursor dan memilih objek di layar.",
                 "Moves the cursor and selects things on the screen."),
                ("Mengklik tombol \"Kirim\" saat mengumpulkan tugas di Google Classroom.",
                 "Clicking \"Submit\" when you hand in homework on Google Classroom."),
                Hs("buttons", ("Tombol Kiri & Kanan", "Left & Right Buttons"),
                    ("Tombol kiri untuk memilih/klik, tombol kanan untuk membuka menu tambahan.",
                     "The left button selects/clicks; the right button opens an extra menu."), new Vector3(-0.08f, 0.2f, 0.14f)),
                Hs("wheel", ("Roda Gulir", "Scroll Wheel"),
                    ("Roda gulir menggulung halaman ke atas atau ke bawah.",
                     "The scroll wheel moves the page up or down."), new Vector3(0f, 0.22f, 0.1f)),
                Hs("sensor", ("Sensor Optik", "Optical Sensor"),
                    ("Sensor cahaya di bawah mouse membaca gerakan pada permukaan meja ribuan kali per detik.",
                     "A light sensor under the mouse reads movement on the desk thousands of times per second."),
                    new Vector3(0f, 0.03f, 0.26f)));

            var monitor = Hw(b, Ids.Monitor, "Monitor", ("Layar Tampilan", "Display Screen"), HardwareCategory.Output, "monitor", "Monitor_Target",
                ("Perangkat output yang menampilkan hasil proses dalam bentuk gambar.",
                 "An output device that shows the results of processing as images."),
                ("Monitor menampilkan informasi dari komputer menggunakan jutaan titik kecil bernama piksel. " +
                 "Setiap piksel tersusun dari warna merah, hijau, dan biru (RGB).",
                 "A monitor shows information from the computer using millions of tiny dots called pixels. " +
                 "Each pixel is made of red, green and blue light (RGB)."),
                ("Menampilkan teks, gambar, dan video hasil proses komputer.",
                 "Displays the text, images and video produced by the computer."),
                ("Menonton video pembelajaran atau melihat hasil ketikanmu di layar.",
                 "Watching a learning video or seeing what you have typed on the screen."),
                Hs("pixels", ("Layar & Piksel", "Screen & Pixels"),
                    ("Gambar di layar tersusun dari piksel. Resolusi 1920×1080 berarti ada lebih dari dua juta piksel.",
                     "The picture is made of pixels. A resolution of 1920×1080 means more than two million pixels."),
                    new Vector3(-0.2f, 0.55f, 0.04f)),
                Hs("port", ("Port Video (HDMI)", "Video Port (HDMI)"),
                    ("Kabel HDMI membawa sinyal gambar dari kartu grafis komputer ke monitor.",
                     "An HDMI cable carries the picture signal from the computer's graphics card to the monitor."),
                    new Vector3(0.3f, 0.4f, -0.05f)),
                Hs("stand", ("Penyangga & Tombol", "Stand & Buttons"),
                    ("Tombol pada monitor untuk menyalakan dan mengatur kecerahan agar nyaman di mata.",
                     "The buttons turn the monitor on and adjust brightness so it is comfortable for your eyes."),
                    new Vector3(0f, 0.12f, 0.1f)));

            var printer = Hw(b, Ids.Printer, "Printer", ("Pencetak", "Printer"), HardwareCategory.Output, "printer", "Printer_Target",
                ("Perangkat output yang mencetak dokumen ke kertas.",
                 "An output device that prints documents on paper."),
                ("Printer menerima data dokumen dari komputer lalu mencetaknya ke kertas menggunakan tinta (inkjet) atau toner (laser).",
                 "A printer receives document data from the computer and prints it on paper using ink (inkjet) or toner (laser)."),
                ("Menghasilkan salinan cetak (hardcopy) dari dokumen digital.",
                 "Produces a printed copy (hard copy) of a digital document."),
                ("Mencetak kartu ujian atau laporan tugas untuk dikumpulkan ke guru.",
                 "Printing an exam card or a report to hand in to your teacher."),
                Hs("tray_in", ("Baki Kertas Masuk", "Paper Input Tray"),
                    ("Kertas kosong diletakkan di sini sebelum ditarik ke dalam printer.",
                     "Blank paper is placed here before it is pulled into the printer."), new Vector3(0f, 0.45f, -0.25f)),
                Hs("head", ("Kepala Cetak / Toner", "Print Head / Toner"),
                    ("Bagian ini menaruh tinta atau toner di atas kertas sesuai data dari komputer.",
                     "This part puts ink or toner on the paper according to the data from the computer."), new Vector3(0.2f, 0.36f, 0.05f)),
                Hs("tray_out", ("Baki Kertas Keluar", "Paper Output Tray"),
                    ("Hasil cetakan keluar di sini sebagai output yang bisa dipegang.",
                     "The printed pages come out here as output you can hold."), new Vector3(0f, 0.12f, 0.35f)));

            var speaker = Hw(b, Ids.Speaker, "Speaker", ("Pengeras Suara", "Loudspeaker"), HardwareCategory.Output, "speaker", "Speaker_Target",
                ("Perangkat output yang menghasilkan suara.",
                 "An output device that produces sound."),
                ("Speaker mengubah sinyal listrik dari komputer menjadi getaran udara yang kita dengar sebagai suara.",
                 "A speaker turns electrical signals from the computer into vibrations in the air that we hear as sound."),
                ("Mengeluarkan audio seperti musik, narasi, dan suara notifikasi.",
                 "Plays audio such as music, narration and notification sounds."),
                ("Mendengarkan narasi pelajaran atau suara video pembelajaran.",
                 "Listening to lesson narration or the sound of a learning video."),
                Hs("woofer", ("Woofer", "Woofer"),
                    ("Woofer adalah membran besar yang menghasilkan suara rendah (bass).",
                     "The woofer is a large cone that produces low sounds (bass)."), new Vector3(0f, 0.25f, 0.22f)),
                Hs("tweeter", ("Tweeter", "Tweeter"),
                    ("Tweeter adalah membran kecil yang menghasilkan suara tinggi yang jernih.",
                     "The tweeter is a small cone that produces clear high sounds."), new Vector3(0f, 0.55f, 0.22f)),
                Hs("amp", ("Kabel Audio", "Audio Cable"),
                    ("Sinyal suara dari komputer masuk melalui kabel audio atau Bluetooth.",
                     "The sound signal from the computer arrives through an audio cable or Bluetooth."), new Vector3(0.15f, 0.1f, -0.22f)));

            var vonNeumann = Hw(b, Ids.VonNeumann, "Von Neumann", ("Arsitektur Von Neumann", "Von Neumann Architecture"), HardwareCategory.Architecture, "workflow", "VonNeumann_Target",
                ("Model dasar cara kerja komputer modern.",
                 "The basic model of how modern computers work."),
                ("Arsitektur Von Neumann menjelaskan bahwa program dan data disimpan bersama di memori. CPU mengambil instruksi " +
                 "dan data dari memori melalui bus, memprosesnya, lalu mengirim hasilnya ke memori atau perangkat output. " +
                 "Komponen utamanya: unit input, CPU (Control Unit dan ALU), memori, penyimpanan, dan unit output.",
                 "The Von Neumann architecture says that programs and data are stored together in memory. The CPU fetches " +
                 "instructions and data from memory over the bus, processes them and sends the results to memory or an output " +
                 "device. Its main parts are the input unit, the CPU (Control Unit and ALU), memory, storage and the output unit."),
                ("Menjelaskan hubungan dan alur data antara input, CPU, memori, penyimpanan, dan output.",
                 "Explains how input, the CPU, memory, storage and output are connected and how data flows between them."),
                ("Saat kamu mengetik huruf \"A\": keyboard (input) › memori › CPU memproses › monitor (output) menampilkan \"A\".",
                 "When you type the letter \"A\": keyboard (input) › memory › the CPU processes it › the monitor (output) shows \"A\"."),
                Hs("input", ("Unit Input", "Input Unit"),
                    ("Data masuk dari perangkat input seperti keyboard dan mouse.",
                     "Data comes in from input devices such as the keyboard and mouse."), new Vector3(-0.42f, 0.3f, 0.1f)),
                Hs("cpu", ("CPU (CU + ALU)", "CPU (CU + ALU)"),
                    ("CPU mengambil, menerjemahkan, dan mengeksekusi instruksi.",
                     "The CPU fetches, decodes and executes instructions."), new Vector3(0f, 0.36f, 0.1f)),
                Hs("memory", ("Memori (RAM)", "Memory (RAM)"),
                    ("Program dan data disimpan bersama di memori — inilah ciri utama Von Neumann.",
                     "Programs and data are stored together in memory — the key idea of Von Neumann."), new Vector3(0f, 0.3f, -0.3f)),
                Hs("output", ("Unit Output", "Output Unit"),
                    ("Hasil proses dikirim ke perangkat output seperti monitor dan printer.",
                     "Results are sent to output devices such as the monitor and printer."), new Vector3(0.42f, 0.3f, 0.1f)),
                Hs("bus", ("Bus Sistem", "System Bus"),
                    ("Bus adalah jalur yang membawa data, alamat, dan sinyal kontrol antar komponen.",
                     "The bus is the pathway that carries data, addresses and control signals between components."),
                    new Vector3(-0.2f, 0.12f, -0.12f)));

            // ------------------------------------------------------------------ Questions
            var q1 = Q(b, "q_cpu_alu", QuestionType.MultipleChoice, cpu,
                ("Bagian CPU yang bertugas melakukan perhitungan dan perbandingan adalah …",
                 "Which part of the CPU carries out calculations and comparisons?"),
                "ALU", ("ALU menangani operasi aritmetika dan logika. Control Unit mengatur, bukan menghitung.",
                        "The ALU handles arithmetic and logic. The Control Unit coordinates; it does not calculate."),
                ("ALU", "ALU"), ("Control Unit", "Control Unit"), ("Register", "Register"), ("Monitor", "Monitor"));
            var q2 = Q(b, "q_vn_tf", QuestionType.TrueFalse, vonNeumann,
                ("Pada arsitektur Von Neumann, program dan data disimpan di memori yang sama.",
                 "In the Von Neumann architecture, programs and data are stored in the same memory."),
                "Benar", ("Benar. Menyimpan program dan data di memori yang sama adalah ciri utama arsitektur Von Neumann.",
                          "True. Storing programs and data in the same memory is the key feature of the Von Neumann architecture."),
                ("Benar", "True"), ("Salah", "False"));
            var q3 = Q(b, "q_cycle_order", QuestionType.Ordering, cpu,
                ("Urutkan siklus kerja CPU dari langkah pertama.", "Put the CPU work cycle in order, starting with the first step."),
                null, ("CPU selalu mengambil instruksi (fetch), menerjemahkannya (decode), lalu mengeksekusinya (execute).",
                       "The CPU always fetches an instruction, decodes it and then executes it."),
                ("Fetch (ambil instruksi)", "Fetch (get the instruction)"), ("Decode (terjemahkan)", "Decode (interpret it)"),
                ("Execute (jalankan)", "Execute (carry it out)"));
            var q4 = Q(b, "q_cpu_ar", QuestionType.ARIdentification, cpu,
                ("Pindai target CPU. Ketuk bagian yang mengatur urutan kerja semua komponen. Bagian apakah itu?",
                 "Scan the CPU target. Tap the part that coordinates the work of all components. Which part is it?"),
                "Control Unit", ("Control Unit (CU) memberi sinyal pengatur ke ALU, memori, dan perangkat input/output.",
                                 "The Control Unit (CU) sends control signals to the ALU, memory and input/output devices."),
                ("Control Unit", "Control Unit"), ("ALU", "ALU"), ("Register & Cache", "Registers & Cache"));

            var q5 = Q(b, "q_ram_volatile", QuestionType.MultipleChoice, memory,
                ("Mengapa tugas yang belum disimpan bisa hilang saat listrik padam?",
                 "Why can unsaved work be lost when the power goes out?"),
                "Karena RAM bersifat volatile",
                ("RAM hanya menyimpan data selama ada listrik. Data yang belum disimpan ke storage akan hilang.",
                 "RAM only keeps data while it has power. Anything not yet saved to storage is lost."),
                ("Karena RAM bersifat volatile", "Because RAM is volatile"), ("Karena storage rusak", "Because the storage is broken"),
                ("Karena CPU terlalu panas", "Because the CPU is too hot"), ("Karena monitor mati", "Because the monitor is off"));
            var q6 = Q(b, "q_storage_id", QuestionType.Identification, storage,
                ("Sebutkan jenis penyimpanan yang memakai chip flash dan lebih cepat daripada HDD (singkatan 3 huruf).",
                 "Name the type of storage that uses flash chips and is faster than an HDD (3-letter abbreviation)."),
                "SSD|solid state drive", ("SSD (Solid State Drive) memakai chip flash tanpa bagian bergerak sehingga lebih cepat daripada HDD.",
                                          "An SSD (Solid State Drive) uses flash chips with no moving parts, so it is faster than an HDD."));
            q6.correctAnswerEn = "SSD|solid state drive|solid-state drive";
            var q7 = Q(b, "q_mem_match", QuestionType.Matching, memory,
                ("Jodohkan komponen dengan sifatnya.", "Match each component with its property."),
                null, ("RAM cepat tetapi sementara; storage permanen; register paling cepat dan paling kecil, berada di dalam CPU.",
                       "RAM is fast but temporary; storage is permanent; registers are the fastest and smallest, inside the CPU."));
            q7.pairs = new List<MatchPair>
            {
                Mp(("RAM", "RAM"), ("Sementara (volatile)", "Temporary (volatile)")),
                Mp(("SSD", "SSD"), ("Permanen (non-volatile)", "Permanent (non-volatile)")),
                Mp(("Register", "Register"), ("Paling cepat, di dalam CPU", "Fastest, inside the CPU"))
            };

            var q8 = Q(b, "q_io_match", QuestionType.Matching, keyboard,
                ("Jodohkan perangkat dengan jenisnya.", "Match each device with its type."),
                null, ("Keyboard dan mouse memasukkan data (input). Monitor dan printer menampilkan/mengeluarkan hasil (output).",
                       "The keyboard and mouse put data in (input). The printer and speaker give results out (output)."));
            q8.pairs = new List<MatchPair>
            {
                Mp(("Keyboard", "Keyboard"), ("Input", "Input")),
                Mp(("Printer", "Printer"), ("Output", "Output")),
                Mp(("Mouse", "Mouse"), ("Input", "Input")),
                Mp(("Speaker", "Speaker"), ("Output", "Output"))
            };
            var q9 = Q(b, "q_io_mc", QuestionType.MultipleChoice, monitor,
                ("Perangkat manakah yang termasuk perangkat output?", "Which of these is an output device?"),
                "Monitor", ("Monitor menampilkan hasil proses komputer, sehingga termasuk perangkat output.",
                            "A monitor displays the computer's results, so it is an output device."),
                ("Keyboard", "Keyboard"), ("Mouse", "Mouse"), ("Monitor", "Monitor"), ("Scanner", "Scanner"));
            var q10 = Q(b, "q_speaker_tf", QuestionType.TrueFalse, speaker,
                ("Speaker adalah perangkat input karena mengeluarkan suara.", "A speaker is an input device because it produces sound."),
                "Salah", ("Salah. Speaker mengeluarkan suara dari komputer, jadi termasuk perangkat output.",
                          "False. A speaker sends sound out of the computer, so it is an output device."),
                ("Benar", "True"), ("Salah", "False"));

            var q11 = Q(b, "q_flow_order", QuestionType.Ordering, vonNeumann,
                ("Urutkan alur data saat kamu mengetik huruf \"A\" hingga muncul di layar.",
                 "Put in order how data flows when you type the letter \"A\" until it appears on the screen."),
                null, ("Data masuk dari keyboard, disimpan di memori, diproses CPU, lalu ditampilkan monitor.",
                       "Data comes in from the keyboard, is stored in memory, is processed by the CPU and is shown on the monitor."),
                ("Keyboard (input)", "Keyboard (input)"), ("Memori (RAM)", "Memory (RAM)"), ("CPU memproses", "CPU processes"),
                ("Monitor (output)", "Monitor (output)"));
            var q12 = Q(b, "q_bus_mc", QuestionType.MultipleChoice, vonNeumann,
                ("Jalur yang membawa data antar komponen komputer disebut …", "The pathway that carries data between computer components is called …"),
                "Bus sistem", ("Bus sistem membawa data, alamat, dan sinyal kontrol antara CPU, memori, dan perangkat I/O.",
                               "The system bus carries data, addresses and control signals between the CPU, memory and I/O devices."),
                ("Bus sistem", "The system bus"), ("Kabel power", "The power cable"), ("Piksel", "A pixel"), ("Register", "A register"));

            // Scenario tasks
            var qs1 = Q(b, "q_sc_video", QuestionType.ContextualScenario, cpu,
                ("Laptop Rani sangat lambat saat merender video dan Task Manager menunjukkan penggunaan CPU 100%. " +
                 "Komponen mana yang paling bekerja keras?",
                 "Rani's laptop is very slow while rendering a video, and Task Manager shows 100% CPU usage. " +
                 "Which component is working hardest?"),
                "CPU", ("Rendering video membutuhkan banyak perhitungan, sehingga CPU (terutama ALU) bekerja sangat keras.",
                        "Rendering video needs a huge number of calculations, so the CPU (especially the ALU) works very hard."),
                ("CPU", "CPU"), ("Speaker", "Speaker"), ("Mouse", "Mouse"), ("Printer", "Printer"));
            var qs2 = Q(b, "q_sc_tabs", QuestionType.ContextualScenario, memory,
                ("Dimas membuka 30 tab browser dan laptopnya mulai tersendat. Upaya paling tepat adalah …",
                 "Dimas opens 30 browser tabs and his laptop starts to stutter. What is the best thing to do?"),
                "Menutup tab yang tidak dipakai agar RAM lega",
                ("Setiap tab memakai RAM. Menutup tab yang tidak dipakai mengosongkan RAM sehingga laptop kembali lancar.",
                 "Every tab uses RAM. Closing unused tabs frees RAM so the laptop runs smoothly again."),
                ("Menutup tab yang tidak dipakai agar RAM lega", "Close unused tabs to free up RAM"), ("Mengganti mouse", "Replace the mouse"),
                ("Menambah kecerahan monitor", "Turn up the monitor brightness"), ("Mencabut speaker", "Unplug the speaker"));
            var qs3 = Q(b, "q_sc_photos", QuestionType.ContextualScenario, storage,
                ("Ponsel Sari menampilkan pesan \"Penyimpanan penuh\" saat menyimpan foto kegiatan kelas. Komponen apa yang penuh?",
                 "Sari's phone shows \"Storage full\" when she saves photos of a class activity. Which component is full?"),
                "Storage", ("Foto disimpan permanen di storage. Pesan itu berarti ruang storage habis, bukan RAM.",
                            "Photos are kept permanently in storage. The message means storage space has run out, not RAM."),
                ("Storage", "Storage"), ("RAM", "RAM"), ("CPU", "CPU"), ("Layar", "Screen"));
            var qs4 = Q(b, "q_sc_cashier", QuestionType.ContextualScenario, keyboard,
                ("Kasir mengetik jumlah barang, lalu struk dicetak. Manakah pasangan input dan output yang tepat?",
                 "A cashier types in the number of items, then a receipt is printed. Which input and output pair is correct?"),
                "Keyboard › Printer", ("Keyboard memasukkan data (input); printer mencetak struk (output).",
                                       "The keyboard enters data (input); the printer prints the receipt (output)."),
                ("Keyboard › Printer", "Keyboard › Printer"), ("Printer › Keyboard", "Printer › Keyboard"),
                ("Monitor › Mouse", "Monitor › Mouse"), ("Speaker › Keyboard", "Speaker › Keyboard"));
            var qs5 = Q(b, "q_sc_typing", QuestionType.ContextualScenario, vonNeumann,
                ("Saat Budi mengetik tugas, di mana huruf yang diketik disimpan sementara sebelum diproses CPU?",
                 "While Budi types his homework, where are the typed letters stored temporarily before the CPU processes them?"),
                "Memori (RAM)", ("Data dari input disimpan dulu di memori (RAM), lalu diambil CPU untuk diproses.",
                                 "Input data is first stored in memory (RAM), then the CPU fetches it for processing."),
                ("Memori (RAM)", "Memory (RAM)"), ("Printer", "Printer"), ("Speaker", "Speaker"), ("Mouse", "Mouse"));

            // ------------------------------------------------------------------ Scenarios
            var s1 = Sc(b, "sc_video_render", ("Laptop Lambat Saat Render Video", "Slow Laptop While Rendering a Video"),
                ("Rumah — mengerjakan tugas video", "Home — working on a video assignment"), "cpu", cpu, qs1,
                ("Rani sedang mengedit video presentasi kelompok. Saat proses render, laptopnya sangat lambat dan kipasnya berbunyi " +
                 "keras. Di Task Manager, penggunaan CPU mencapai 100%.",
                 "Rani is editing her group's presentation video. While it renders, her laptop becomes very slow and the fan is " +
                 "loud. Task Manager shows CPU usage at 100%."),
                ("Komponen apa yang sedang memproses jutaan perhitungan untuk menyusun video?",
                 "Which component is processing millions of calculations to build the video?"),
                ("Tepat sekali jika kamu memilih CPU. Render video terdiri atas banyak perhitungan yang dikerjakan ALU, " +
                 "sementara Control Unit mengatur urutannya. Menutup aplikasi lain dapat memberi CPU lebih banyak waktu untuk render.",
                 "The CPU is the right answer. Rendering video is made of many calculations done by the ALU, while the Control " +
                 "Unit organises their order. Closing other apps gives the CPU more time for rendering."),
                ("Kapan kamu pernah merasakan komputer lambat karena CPU bekerja keras? Apa yang bisa kamu lakukan?",
                 "When have you noticed a computer slow down because the CPU was working hard? What could you do about it?"));
            var s2 = Sc(b, "sc_browser_tabs", ("Terlalu Banyak Tab Browser", "Too Many Browser Tabs"),
                ("Lab komputer sekolah", "School computer lab"), "memory-stick", memory, qs2,
                ("Dimas mencari referensi tugas dan membuka 30 tab browser. Lama-lama laptop tersendat, bahkan mengetik pun terasa lambat.",
                 "Dimas is researching his assignment and opens 30 browser tabs. Soon the laptop stutters and even typing feels slow."),
                ("Di mana semua tab yang terbuka itu disimpan sementara?", "Where are all those open tabs stored temporarily?"),
                ("Setiap tab browser disimpan di RAM. Jika RAM penuh, komputer memindahkan data ke storage yang jauh lebih lambat. " +
                 "Menutup tab yang tidak dipakai membuat RAM lega kembali.",
                 "Every browser tab is kept in RAM. When RAM is full, the computer moves data to much slower storage. " +
                 "Closing unused tabs frees up RAM again."),
                ("Bagaimana kamu akan mengatur tab dan aplikasi saat belajar agar komputer tetap lancar?",
                 "How will you manage tabs and apps while studying so the computer keeps running smoothly?"));
            var s3 = Sc(b, "sc_storage_full", ("Penyimpanan Penuh", "Storage Full"),
                ("Kegiatan kelas di luar sekolah", "Class trip outside school"), "hard-drive", storage, qs3,
                ("Sari ingin menyimpan foto kegiatan kelas, tetapi ponselnya menampilkan pesan \"Penyimpanan penuh\".",
                 "Sari wants to save photos of a class activity, but her phone shows the message \"Storage full\"."),
                ("Komponen mana yang menyimpan foto secara permanen?", "Which component keeps photos permanently?"),
                ("Foto disimpan di storage agar tetap ada setelah ponsel dimatikan. Menghapus file yang tidak perlu atau " +
                 "memindahkannya ke cloud akan mengosongkan storage.",
                 "Photos are kept in storage so they are still there after the phone is switched off. Deleting files you do not " +
                 "need, or moving them to the cloud, frees up storage."),
                ("File apa yang biasanya memenuhi penyimpanan perangkatmu, dan bagaimana kamu mengelolanya?",
                 "What kinds of files usually fill up your device's storage, and how do you manage them?"));
            var s4 = Sc(b, "sc_cashier", ("Kasir Minimarket", "Minimarket Cashier"),
                ("Minimarket dekat sekolah", "Minimarket near school"), "printer", keyboard, qs4,
                ("Di minimarket, kasir mengetik jumlah barang, total belanja tampil di layar, lalu struk dicetak untuk pembeli.",
                 "At the minimarket, the cashier types in the number of items, the total appears on the screen and a receipt is printed."),
                ("Perangkat mana yang memasukkan data, dan perangkat mana yang mengeluarkan hasil?",
                 "Which devices put data in, and which devices give results out?"),
                ("Keyboard dan pemindai barcode adalah perangkat input. Monitor, printer struk, dan speaker adalah perangkat output. " +
                 "CPU memproses total belanja di antaranya.",
                 "The keyboard and barcode scanner are input devices. The monitor, receipt printer and speaker are output devices. " +
                 "The CPU calculates the total in between."),
                ("Sebutkan satu tempat lain di sekitarmu yang memakai perangkat input dan output. Apa saja perangkatnya?",
                 "Name another place around you that uses input and output devices. Which devices are they?"));
            var s5 = Sc(b, "sc_typing_flow", ("Dari Ketikan ke Layar", "From Keystroke to Screen"),
                ("Mengerjakan tugas di komputer", "Doing homework on the computer"), "workflow", vonNeumann, qs5,
                ("Budi mengetik tugas bahasa Indonesia. Setiap huruf yang ditekan langsung muncul di layar dalam sekejap.",
                 "Budi is typing his Indonesian homework. Every letter he presses appears on the screen in an instant."),
                ("Melewati komponen apa saja sebuah huruf sebelum muncul di layar?",
                 "Which components does a letter pass through before it appears on the screen?"),
                ("Huruf dari keyboard (input) disimpan di memori, diambil dan diproses CPU, lalu hasilnya dikirim ke monitor (output). " +
                 "Semua melewati bus sistem sesuai arsitektur Von Neumann.",
                 "The letter from the keyboard (input) is stored in memory, fetched and processed by the CPU, and the result is sent " +
                 "to the monitor (output). Everything travels over the system bus, as in the Von Neumann architecture."),
                ("Jelaskan dengan kata-katamu sendiri alur data ketika kamu mengirim pesan di ponsel.",
                 "In your own words, explain how data flows when you send a message on your phone."));

            // ------------------------------------------------------------------ Modules
            var m1 = Mod(b, "m1_cpu_vn", 1, ("Arsitektur Von Neumann & CPU", "Von Neumann Architecture & the CPU"), "cpu", 25,
                ("Kenali model Von Neumann dan cara CPU menjalankan instruksi melalui AR.",
                 "Discover the Von Neumann model and how the CPU runs instructions, through AR."),
                ("Bagaimana pemahamanmu tentang tugas CPU dan hubungannya dengan memori setelah pelajaran ini?",
                 "After this lesson, how well do you understand the CPU's job and how it works with memory?"),
                new[]
                {
                    ("Menjelaskan komponen utama arsitektur Von Neumann.", "Explain the main components of the Von Neumann architecture."),
                    ("Membedakan fungsi Control Unit, ALU, dan register di dalam CPU.", "Distinguish the roles of the Control Unit, ALU and registers in the CPU."),
                    ("Menghubungkan kerja CPU dengan situasi nyata sehari-hari.", "Connect how the CPU works to everyday situations.")
                },
                new[] { vonNeumann, cpu }, new[] { s1 }, new[] { q1, q2, q3, q4 },
                Step(LessonStepType.VonNeumann), Step(LessonStepType.AR, cpu), Step(LessonStepType.Scenario, null, s1),
                Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            var m2 = Mod(b, "m2_memory_storage", 2, ("Memori & Penyimpanan", "Memory & Storage"), "memory-stick", 20,
                ("Bandingkan RAM dan storage, serta pahami mengapa data bisa hilang.",
                 "Compare RAM and storage, and understand why data can be lost."),
                ("Apa perbedaan RAM dan storage menurut kata-katamu sendiri?",
                 "In your own words, what is the difference between RAM and storage?"),
                new[]
                {
                    ("Membedakan memori volatile (RAM) dan non-volatile (storage).", "Distinguish volatile memory (RAM) from non-volatile storage."),
                    ("Menjelaskan peran RAM saat program berjalan.", "Explain the role of RAM while programs are running."),
                    ("Menerapkan cara mengelola memori dan penyimpanan perangkat.", "Apply ways to manage a device's memory and storage.")
                },
                new[] { memory, storage }, new[] { s2, s3 }, new[] { q5, q6, q7 },
                Step(LessonStepType.AR, memory), Step(LessonStepType.Scenario, null, s2), Step(LessonStepType.AR, storage),
                Step(LessonStepType.Scenario, null, s3), Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            var m3 = Mod(b, "m3_input_output", 3, ("Perangkat Input & Output", "Input & Output Devices"), "keyboard", 20,
                ("Jelajahi perangkat yang memasukkan data dan yang menampilkan hasil.",
                 "Explore the devices that put data in and the ones that show results."),
                ("Perangkat input dan output apa yang paling sering kamu gunakan, dan untuk apa?",
                 "Which input and output devices do you use most often, and what for?"),
                new[]
                {
                    ("Mengelompokkan perangkat ke dalam input dan output.", "Sort devices into input and output."),
                    ("Menjelaskan fungsi keyboard, mouse, monitor, printer, dan speaker.", "Explain what the keyboard, mouse, monitor, printer and speaker do."),
                    ("Mengidentifikasi perangkat I/O dalam situasi nyata.", "Identify I/O devices in real-life situations.")
                },
                new[] { keyboard, mouse, monitor, printer, speaker }, new[] { s4 }, new[] { q8, q9, q10 },
                Step(LessonStepType.HardwareExplorer), Step(LessonStepType.AR, keyboard), Step(LessonStepType.Scenario, null, s4),
                Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            var m4 = Mod(b, "m4_data_flow", 4, ("Alur Data Antar Komponen", "How Data Flows Between Components"), "workflow", 20,
                ("Ikuti perjalanan data dari input, memori, CPU, hingga output.",
                 "Follow data on its journey from input through memory and the CPU to output."),
                ("Setelah melihat alur data, bagian mana yang paling mudah dan paling sulit kamu pahami?",
                 "Now that you have seen the data flow, which part was easiest and which was hardest to understand?"),
                new[]
                {
                    ("Menjelaskan alur data input › memori › CPU › output.", "Explain the data flow input › memory › CPU › output."),
                    ("Menjelaskan peran bus sistem.", "Explain the role of the system bus."),
                    ("Menerapkan konsep alur data pada contoh nyata.", "Apply the idea of data flow to real examples.")
                },
                new[] { vonNeumann }, new[] { s5 }, new[] { q11, q12 },
                Step(LessonStepType.VonNeumann), Step(LessonStepType.AR, vonNeumann), Step(LessonStepType.Scenario, null, s5),
                Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            foreach (var m in new[] { m1, m2, m3, m4 })
                foreach (var q in m.questions) q.relatedModuleId = m.moduleId;

            return b;
        }

        // ------------------------------------------------------------------ builders
        private static HardwareData Hw(Bundle b, string id, string name, (string id, string en) fullName, HardwareCategory cat,
            string icon, string target, (string id, string en) shortDesc, (string id, string en) detail, (string id, string en) function,
            (string id, string en) context, params HotspotData[] hotspots)
        {
            var h = ScriptableObject.CreateInstance<HardwareData>();
            h.name = "Hardware_" + id;
            h.hardwareId = id;
            h.hardwareName = name;
            h.category = cat;
            h.iconName = icon;
            h.arTargetName = target;
            (h.fullName, h.fullNameEn) = fullName;
            (h.shortDescription, h.shortDescriptionEn) = shortDesc;
            (h.detailedDescription, h.detailedDescriptionEn) = detail;
            (h.function, h.functionEn) = function;
            (h.contextualExample, h.contextualExampleEn) = context;
            h.hotspots = new List<HotspotData>(hotspots);
            h.printedWidthMeters = AppConstants.TargetPrintedWidthMeters;
            b.Hardware.Add(h);
            return h;
        }

        private static HotspotData Hs(string id, (string id, string en) label, (string id, string en) description, Vector3 pos) =>
            new HotspotData
            {
                hotspotId = id, label = label.id, labelEn = label.en,
                description = description.id, descriptionEn = description.en, localPosition = pos
            };

        private static MatchPair Mp((string id, string en) left, (string id, string en) right) =>
            new MatchPair { left = left.id, leftEn = left.en, right = right.id, rightEn = right.en };

        /// <param name="correct">Indonesian option text of the correct answer (or accepted answers for Identification).</param>
        private static QuestionData Q(Bundle b, string id, QuestionType type, HardwareData hw, (string id, string en) text, string correct,
            (string id, string en) explanation, params (string id, string en)[] options)
        {
            var q = ScriptableObject.CreateInstance<QuestionData>();
            q.name = "Question_" + id;
            q.questionId = id;
            q.questionType = type;
            q.relatedHardware = hw;
            q.iconName = hw != null ? hw.iconName : null;
            (q.questionText, q.questionTextEn) = text;
            q.correctAnswer = correct;
            (q.explanation, q.explanationEn) = explanation;
            q.options = options.Select(o => o.id).ToList();
            q.optionsEn = options.Select(o => o.en).ToList();
            b.Questions.Add(q);
            return q;
        }

        private static ScenarioData Sc(Bundle b, string id, (string id, string en) title, (string id, string en) env, string icon,
            HardwareData hw, QuestionData task, (string id, string en) description, (string id, string en) question,
            (string id, string en) feedback, (string id, string en) reflection)
        {
            var s = ScriptableObject.CreateInstance<ScenarioData>();
            s.name = "Scenario_" + id;
            s.scenarioId = id;
            (s.title, s.titleEn) = title;
            (s.environment, s.environmentEn) = env;
            s.iconName = icon;
            s.hardware = hw;
            s.ARTarget = hw.arTargetName;
            s.activity = task;
            (s.description, s.descriptionEn) = description;
            (s.question, s.questionEn) = question;
            (s.feedback, s.feedbackEn) = feedback;
            (s.reflection, s.reflectionEn) = reflection;
            b.Scenarios.Add(s);
            return s;
        }

        private static LessonStep Step(LessonStepType type, HardwareData hw = null, ScenarioData sc = null) =>
            new LessonStep { type = type, hardware = hw, scenario = sc };

        private static ModuleData Mod(Bundle b, string id, int order, (string id, string en) title, string icon, int minutes,
            (string id, string en) desc, (string id, string en) reflectionPrompt, (string id, string en)[] objectives,
            HardwareData[] hw, ScenarioData[] scenarios, QuestionData[] questions, params LessonStep[] steps)
        {
            var m = ScriptableObject.CreateInstance<ModuleData>();
            m.name = "Module_" + id;
            m.moduleId = id;
            m.order = order;
            (m.title, m.titleEn) = title;
            m.iconName = icon;
            m.estimatedMinutes = minutes;
            (m.description, m.descriptionEn) = desc;
            (m.reflectionPrompt, m.reflectionPromptEn) = reflectionPrompt;
            m.learningObjective = objectives.Select(o => o.id).ToList();
            m.learningObjectiveEn = objectives.Select(o => o.en).ToList();
            m.hardware = new List<HardwareData>(hw);
            m.scenarios = new List<ScenarioData>(scenarios);
            m.questions = new List<QuestionData>(questions);
            m.lessonSteps = new List<LessonStep>(steps);
            b.Modules.Add(m);
            return m;
        }
    }
}
