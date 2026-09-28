using System.Collections.Generic;
using ComputerExplorer.Core;
using UnityEngine;
using Ids = ComputerExplorer.Core.AppConstants.HardwareIds;

namespace ComputerExplorer.Data
{
    /// <summary>
    /// Seed educational content (Bahasa Indonesia). Used directly when no content assets exist, and
    /// by the editor menu "Computer Explorer > Create Content Assets" to generate editable
    /// ScriptableObjects in Assets/Resources/Content. After generating assets, edit the assets —
    /// not this file.
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
            var cpu = Hw(b, Ids.Cpu, "CPU", "Central Processing Unit", HardwareCategory.Processing, "cpu", "CPU_Target",
                "Otak komputer yang menjalankan instruksi program.",
                "CPU (Central Processing Unit) mengambil instruksi dari memori, menerjemahkannya, lalu mengeksekusinya. " +
                "Di dalam CPU terdapat Control Unit (CU) yang mengatur urutan kerja, Arithmetic Logic Unit (ALU) yang menghitung " +
                "dan membandingkan data, serta register sebagai tempat penyimpanan sangat cepat berukuran kecil. " +
                "Siklus kerjanya disebut fetch – decode – execute.",
                "Memproses instruksi dan data: menghitung, membandingkan, dan mengatur kerja komponen lain.",
                "Saat kamu menekan tombol \"=\" di aplikasi kalkulator, ALU di dalam CPU yang menghitung hasilnya.",
                Hs("alu", "ALU (Arithmetic Logic Unit)",
                    "ALU melakukan operasi aritmetika (tambah, kurang, kali, bagi) dan operasi logika (membandingkan, AND, OR). " +
                    "Contoh: menghitung nilai rata-rata rapor.", new Vector3(-0.17f, 0.2f, 0.12f)),
                Hs("control_unit", "Control Unit (CU)",
                    "Control Unit mengambil instruksi dari memori, menerjemahkannya, lalu memberi sinyal kepada ALU, memori, " +
                    "dan perangkat input/output. Ia seperti dirigen yang mengatur orkestra.", new Vector3(0.17f, 0.2f, 0.12f)),
                Hs("registers", "Register & Cache",
                    "Register dan cache adalah memori super cepat di dalam CPU. Data yang sedang diproses disimpan di sini " +
                    "agar CPU tidak perlu menunggu RAM.", new Vector3(0f, 0.2f, -0.16f)));

            var memory = Hw(b, Ids.Memory, "RAM", "Random Access Memory", HardwareCategory.Memory, "memory-stick", "Memory_Target",
                "Memori kerja sementara untuk program dan data yang sedang dipakai.",
                "RAM menyimpan program dan data yang sedang digunakan agar CPU dapat mengaksesnya dengan cepat. " +
                "RAM bersifat volatile: isinya hilang ketika komputer dimatikan. Semakin besar kapasitas RAM, semakin banyak " +
                "program yang dapat dibuka bersamaan tanpa melambat.",
                "Menyimpan sementara instruksi dan data yang sedang diproses CPU.",
                "Saat kamu membuka banyak tab browser sekaligus, setiap tab memakai ruang di RAM.",
                Hs("chips", "Chip Memori",
                    "Chip-chip hitam ini berisi jutaan sel memori kecil tempat data disimpan sementara dalam bentuk bit 0 dan 1.",
                    new Vector3(-0.22f, 0.24f, 0.06f)),
                Hs("contacts", "Pin Kontak Emas",
                    "Pin kontak menghubungkan modul RAM ke slot di motherboard sehingga data dapat mengalir ke CPU.",
                    new Vector3(0.25f, 0.05f, 0.06f)),
                Hs("volatile", "Sifat Volatile",
                    "RAM hanya menyimpan data selama ada listrik. Karena itu, simpan pekerjaanmu ke storage sebelum mematikan komputer.",
                    new Vector3(0.3f, 0.3f, 0.06f)));

            var storage = Hw(b, Ids.Storage, "Storage", "Penyimpanan (SSD / HDD)", HardwareCategory.Storage, "hard-drive", "Storage_Target",
                "Tempat menyimpan file dan program secara permanen.",
                "Storage seperti SSD dan HDD menyimpan sistem operasi, aplikasi, dan file secara permanen (non-volatile), " +
                "sehingga data tetap ada walaupun komputer dimatikan. SSD memakai chip flash sehingga lebih cepat daripada HDD " +
                "yang memakai piringan magnetik berputar.",
                "Menyimpan data secara permanen dan memuatnya ke RAM ketika dibutuhkan.",
                "Foto dan dokumen tugas sekolah yang kamu simpan tetap ada besok karena tersimpan di storage.",
                Hs("nand", "Chip Flash (NAND)",
                    "Chip flash menyimpan data tanpa memerlukan listrik, sehingga file tetap aman setelah komputer dimatikan.",
                    new Vector3(-0.2f, 0.1f, 0f)),
                Hs("controller", "Controller",
                    "Controller mengatur di mana data ditulis dan dibaca, serta menjaga agar chip flash awet.",
                    new Vector3(0.12f, 0.1f, 0f)),
                Hs("connector", "Konektor",
                    "Konektor menghubungkan storage ke motherboard. Data dari storage dimuat ke RAM sebelum diproses CPU.",
                    new Vector3(0.46f, 0.08f, 0f)));

            var keyboard = Hw(b, Ids.Keyboard, "Keyboard", "Papan Ketik", HardwareCategory.Input, "keyboard", "Keyboard_Target",
                "Perangkat input untuk mengetik huruf, angka, dan perintah.",
                "Keyboard mengubah setiap tekanan tombol menjadi kode digital yang dikirim ke komputer. " +
                "Kode ini masuk ke memori, lalu diproses CPU sehingga huruf muncul di layar.",
                "Memasukkan data teks dan perintah ke komputer.",
                "Mengetik nama dan alamat saat mengisi formulir pendaftaran online.",
                Hs("letters", "Tombol Huruf & Angka",
                    "Setiap tombol memiliki kode unik. Saat ditekan, kode dikirim ke komputer sebagai input.",
                    new Vector3(-0.15f, 0.12f, 0.02f)),
                Hs("enter", "Tombol Enter",
                    "Tombol Enter mengirim perintah untuk menjalankan atau mengonfirmasi, misalnya mengirim pesan.",
                    new Vector3(0.38f, 0.12f, 0f)),
                Hs("cable", "Kabel / Koneksi",
                    "Data dari keyboard dikirim melalui kabel USB atau secara nirkabel (Bluetooth) ke komputer.",
                    new Vector3(0f, 0.1f, -0.27f)));

            var mouse = Hw(b, Ids.Mouse, "Mouse", "Tetikus", HardwareCategory.Input, "mouse", "Mouse_Target",
                "Perangkat input penunjuk untuk menggerakkan kursor.",
                "Mouse mendeteksi gerakan tangan melalui sensor optik dan mengirimkan posisi serta klik ke komputer.",
                "Menggerakkan kursor dan memilih objek di layar.",
                "Mengklik tombol \"Kirim\" saat mengumpulkan tugas di Google Classroom.",
                Hs("buttons", "Tombol Kiri & Kanan",
                    "Tombol kiri untuk memilih/klik, tombol kanan untuk membuka menu tambahan.", new Vector3(-0.08f, 0.2f, 0.14f)),
                Hs("wheel", "Roda Gulir",
                    "Roda gulir menggulung halaman ke atas atau ke bawah.", new Vector3(0f, 0.22f, 0.1f)),
                Hs("sensor", "Sensor Optik",
                    "Sensor cahaya di bawah mouse membaca gerakan pada permukaan meja ribuan kali per detik.",
                    new Vector3(0f, 0.03f, 0.26f)));

            var monitor = Hw(b, Ids.Monitor, "Monitor", "Layar Tampilan", HardwareCategory.Output, "monitor", "Monitor_Target",
                "Perangkat output yang menampilkan hasil proses dalam bentuk gambar.",
                "Monitor menampilkan informasi dari komputer menggunakan jutaan titik kecil bernama piksel. " +
                "Setiap piksel tersusun dari warna merah, hijau, dan biru (RGB).",
                "Menampilkan teks, gambar, dan video hasil proses komputer.",
                "Menonton video pembelajaran atau melihat hasil ketikanmu di layar.",
                Hs("pixels", "Layar & Piksel",
                    "Gambar di layar tersusun dari piksel. Resolusi 1920×1080 berarti ada lebih dari dua juta piksel.",
                    new Vector3(-0.2f, 0.55f, 0.04f)),
                Hs("port", "Port Video (HDMI)",
                    "Kabel HDMI membawa sinyal gambar dari kartu grafis komputer ke monitor.", new Vector3(0.3f, 0.4f, -0.05f)),
                Hs("stand", "Penyangga & Tombol",
                    "Tombol pada monitor untuk menyalakan dan mengatur kecerahan agar nyaman di mata.",
                    new Vector3(0f, 0.12f, 0.1f)));

            var printer = Hw(b, Ids.Printer, "Printer", "Pencetak", HardwareCategory.Output, "printer", "Printer_Target",
                "Perangkat output yang mencetak dokumen ke kertas.",
                "Printer menerima data dokumen dari komputer lalu mencetaknya ke kertas menggunakan tinta (inkjet) atau toner (laser).",
                "Menghasilkan salinan cetak (hardcopy) dari dokumen digital.",
                "Mencetak kartu ujian atau laporan tugas untuk dikumpulkan ke guru.",
                Hs("tray_in", "Baki Kertas Masuk",
                    "Kertas kosong diletakkan di sini sebelum ditarik ke dalam printer.", new Vector3(0f, 0.45f, -0.25f)),
                Hs("head", "Kepala Cetak / Toner",
                    "Bagian ini menaruh tinta atau toner di atas kertas sesuai data dari komputer.", new Vector3(0.2f, 0.36f, 0.05f)),
                Hs("tray_out", "Baki Kertas Keluar",
                    "Hasil cetakan keluar di sini sebagai output yang bisa dipegang.", new Vector3(0f, 0.12f, 0.35f)));

            var speaker = Hw(b, Ids.Speaker, "Speaker", "Pengeras Suara", HardwareCategory.Output, "speaker", "Speaker_Target",
                "Perangkat output yang menghasilkan suara.",
                "Speaker mengubah sinyal listrik dari komputer menjadi getaran udara yang kita dengar sebagai suara.",
                "Mengeluarkan audio seperti musik, narasi, dan suara notifikasi.",
                "Mendengarkan narasi pelajaran atau suara video pembelajaran.",
                Hs("woofer", "Woofer",
                    "Woofer adalah membran besar yang menghasilkan suara rendah (bass).", new Vector3(0f, 0.25f, 0.22f)),
                Hs("tweeter", "Tweeter",
                    "Tweeter adalah membran kecil yang menghasilkan suara tinggi yang jernih.", new Vector3(0f, 0.55f, 0.22f)),
                Hs("amp", "Kabel Audio",
                    "Sinyal suara dari komputer masuk melalui kabel audio atau Bluetooth.", new Vector3(0.15f, 0.1f, -0.22f)));

            var vonNeumann = Hw(b, Ids.VonNeumann, "Von Neumann", "Arsitektur Von Neumann", HardwareCategory.Architecture, "workflow", "VonNeumann_Target",
                "Model dasar cara kerja komputer modern.",
                "Arsitektur Von Neumann menjelaskan bahwa program dan data disimpan bersama di memori. CPU mengambil instruksi " +
                "dan data dari memori melalui bus, memprosesnya, lalu mengirim hasilnya ke memori atau perangkat output. " +
                "Komponen utamanya: unit input, CPU (Control Unit dan ALU), memori, penyimpanan, dan unit output.",
                "Menjelaskan hubungan dan alur data antara input, CPU, memori, penyimpanan, dan output.",
                "Saat kamu mengetik huruf \"A\": keyboard (input) › memori › CPU memproses › monitor (output) menampilkan \"A\".",
                Hs("input", "Unit Input", "Data masuk dari perangkat input seperti keyboard dan mouse.", new Vector3(-0.42f, 0.3f, 0.1f)),
                Hs("cpu", "CPU (CU + ALU)", "CPU mengambil, menerjemahkan, dan mengeksekusi instruksi.", new Vector3(0f, 0.36f, 0.1f)),
                Hs("memory", "Memori (RAM)", "Program dan data disimpan bersama di memori — inilah ciri utama Von Neumann.", new Vector3(0f, 0.3f, -0.3f)),
                Hs("output", "Unit Output", "Hasil proses dikirim ke perangkat output seperti monitor dan printer.", new Vector3(0.42f, 0.3f, 0.1f)),
                Hs("bus", "Bus Sistem", "Bus adalah jalur yang membawa data, alamat, dan sinyal kontrol antar komponen.", new Vector3(-0.2f, 0.12f, -0.12f)));

            // ------------------------------------------------------------------ Questions
            var q1 = Q(b, "q_cpu_alu", QuestionType.MultipleChoice, cpu,
                "Bagian CPU yang bertugas melakukan perhitungan dan perbandingan adalah …",
                "ALU", "ALU menangani operasi aritmetika dan logika. Control Unit mengatur, bukan menghitung.",
                "ALU", "Control Unit", "Register", "Monitor");
            var q2 = Q(b, "q_vn_tf", QuestionType.TrueFalse, vonNeumann,
                "Pada arsitektur Von Neumann, program dan data disimpan di memori yang sama.",
                "Benar", "Benar. Menyimpan program dan data di memori yang sama adalah ciri utama arsitektur Von Neumann.",
                "Benar", "Salah");
            var q3 = Q(b, "q_cycle_order", QuestionType.Ordering, cpu,
                "Urutkan siklus kerja CPU dari langkah pertama.",
                null, "CPU selalu mengambil instruksi (fetch), menerjemahkannya (decode), lalu mengeksekusinya (execute).",
                "Fetch (ambil instruksi)", "Decode (terjemahkan)", "Execute (jalankan)");
            var q4 = Q(b, "q_cpu_ar", QuestionType.ARIdentification, cpu,
                "Pindai target CPU. Ketuk bagian yang mengatur urutan kerja semua komponen. Bagian apakah itu?",
                "Control Unit", "Control Unit (CU) memberi sinyal pengatur ke ALU, memori, dan perangkat input/output.",
                "Control Unit", "ALU", "Register & Cache");

            var q5 = Q(b, "q_ram_volatile", QuestionType.MultipleChoice, memory,
                "Mengapa tugas yang belum disimpan bisa hilang saat listrik padam?",
                "Karena RAM bersifat volatile",
                "RAM hanya menyimpan data selama ada listrik. Data yang belum disimpan ke storage akan hilang.",
                "Karena RAM bersifat volatile", "Karena storage rusak", "Karena CPU terlalu panas", "Karena monitor mati");
            var q6 = Q(b, "q_storage_id", QuestionType.Identification, storage,
                "Sebutkan jenis penyimpanan yang memakai chip flash dan lebih cepat daripada HDD (singkatan 3 huruf).",
                "SSD|solid state drive", "SSD (Solid State Drive) memakai chip flash tanpa bagian bergerak sehingga lebih cepat daripada HDD.");
            var q7 = Q(b, "q_mem_match", QuestionType.Matching, memory,
                "Jodohkan komponen dengan sifatnya.",
                null, "RAM cepat tetapi sementara; storage permanen; register paling cepat dan paling kecil, berada di dalam CPU.");
            q7.pairs = new List<MatchPair>
            {
                new MatchPair { left = "RAM", right = "Sementara (volatile)" },
                new MatchPair { left = "SSD", right = "Permanen (non-volatile)" },
                new MatchPair { left = "Register", right = "Paling cepat, di dalam CPU" }
            };

            var q8 = Q(b, "q_io_match", QuestionType.Matching, keyboard,
                "Jodohkan perangkat dengan jenisnya.",
                null, "Keyboard dan mouse memasukkan data (input). Monitor dan printer menampilkan/mengeluarkan hasil (output).");
            q8.pairs = new List<MatchPair>
            {
                new MatchPair { left = "Keyboard", right = "Input" },
                new MatchPair { left = "Printer", right = "Output" },
                new MatchPair { left = "Mouse", right = "Input" },
                new MatchPair { left = "Speaker", right = "Output" }
            };
            var q9 = Q(b, "q_io_mc", QuestionType.MultipleChoice, monitor,
                "Perangkat manakah yang termasuk perangkat output?",
                "Monitor", "Monitor menampilkan hasil proses komputer, sehingga termasuk perangkat output.",
                "Keyboard", "Mouse", "Monitor", "Scanner");
            var q10 = Q(b, "q_speaker_tf", QuestionType.TrueFalse, speaker,
                "Speaker adalah perangkat input karena mengeluarkan suara.",
                "Salah", "Salah. Speaker mengeluarkan suara dari komputer, jadi termasuk perangkat output.",
                "Benar", "Salah");

            var q11 = Q(b, "q_flow_order", QuestionType.Ordering, vonNeumann,
                "Urutkan alur data saat kamu mengetik huruf \"A\" hingga muncul di layar.",
                null, "Data masuk dari keyboard, disimpan di memori, diproses CPU, lalu ditampilkan monitor.",
                "Keyboard (input)", "Memori (RAM)", "CPU memproses", "Monitor (output)");
            var q12 = Q(b, "q_bus_mc", QuestionType.MultipleChoice, vonNeumann,
                "Jalur yang membawa data antar komponen komputer disebut …",
                "Bus sistem", "Bus sistem membawa data, alamat, dan sinyal kontrol antara CPU, memori, dan perangkat I/O.",
                "Bus sistem", "Kabel power", "Piksel", "Register");

            // Scenario tasks
            var qs1 = Q(b, "q_sc_video", QuestionType.ContextualScenario, cpu,
                "Laptop Rani sangat lambat saat merender video dan Task Manager menunjukkan penggunaan CPU 100%. " +
                "Komponen mana yang paling bekerja keras?",
                "CPU", "Rendering video membutuhkan banyak perhitungan, sehingga CPU (terutama ALU) bekerja sangat keras.",
                "CPU", "Speaker", "Mouse", "Printer");
            var qs2 = Q(b, "q_sc_tabs", QuestionType.ContextualScenario, memory,
                "Dimas membuka 30 tab browser dan laptopnya mulai tersendat. Upaya paling tepat adalah …",
                "Menutup tab yang tidak dipakai agar RAM lega",
                "Setiap tab memakai RAM. Menutup tab yang tidak dipakai mengosongkan RAM sehingga laptop kembali lancar.",
                "Menutup tab yang tidak dipakai agar RAM lega", "Mengganti mouse", "Menambah kecerahan monitor", "Mencabut speaker");
            var qs3 = Q(b, "q_sc_photos", QuestionType.ContextualScenario, storage,
                "Ponsel Sari menampilkan pesan \"Penyimpanan penuh\" saat menyimpan foto kegiatan kelas. Komponen apa yang penuh?",
                "Storage", "Foto disimpan permanen di storage. Pesan itu berarti ruang storage habis, bukan RAM.",
                "Storage", "RAM", "CPU", "Layar");
            var qs4 = Q(b, "q_sc_cashier", QuestionType.ContextualScenario, keyboard,
                "Kasir mengetik jumlah barang, lalu struk dicetak. Manakah pasangan input dan output yang tepat?",
                "Keyboard › Printer", "Keyboard memasukkan data (input); printer mencetak struk (output).",
                "Keyboard › Printer", "Printer › Keyboard", "Monitor › Mouse", "Speaker › Keyboard");
            var qs5 = Q(b, "q_sc_typing", QuestionType.ContextualScenario, vonNeumann,
                "Saat Budi mengetik tugas, di mana huruf yang diketik disimpan sementara sebelum diproses CPU?",
                "Memori (RAM)", "Data dari input disimpan dulu di memori (RAM), lalu diambil CPU untuk diproses.",
                "Memori (RAM)", "Printer", "Speaker", "Mouse");

            // ------------------------------------------------------------------ Scenarios
            var s1 = Sc(b, "sc_video_render", "Laptop Lambat Saat Render Video", "Rumah — mengerjakan tugas video", "cpu", cpu, qs1,
                "Rani sedang mengedit video presentasi kelompok. Saat proses render, laptopnya sangat lambat dan kipasnya berbunyi " +
                "keras. Di Task Manager, penggunaan CPU mencapai 100%.",
                "Komponen apa yang sedang memproses jutaan perhitungan untuk menyusun video?",
                "Tepat sekali jika kamu memilih CPU. Render video terdiri atas banyak perhitungan yang dikerjakan ALU, " +
                "sementara Control Unit mengatur urutannya. Menutup aplikasi lain dapat memberi CPU lebih banyak waktu untuk render.",
                "Kapan kamu pernah merasakan komputer lambat karena CPU bekerja keras? Apa yang bisa kamu lakukan?");
            var s2 = Sc(b, "sc_browser_tabs", "Terlalu Banyak Tab Browser", "Lab komputer sekolah", "memory-stick", memory, qs2,
                "Dimas mencari referensi tugas dan membuka 30 tab browser. Lama-lama laptop tersendat, bahkan mengetik pun terasa lambat.",
                "Di mana semua tab yang terbuka itu disimpan sementara?",
                "Setiap tab browser disimpan di RAM. Jika RAM penuh, komputer memindahkan data ke storage yang jauh lebih lambat. " +
                "Menutup tab yang tidak dipakai membuat RAM lega kembali.",
                "Bagaimana kamu akan mengatur tab dan aplikasi saat belajar agar komputer tetap lancar?");
            var s3 = Sc(b, "sc_storage_full", "Penyimpanan Penuh", "Kegiatan kelas di luar sekolah", "hard-drive", storage, qs3,
                "Sari ingin menyimpan foto kegiatan kelas, tetapi ponselnya menampilkan pesan \"Penyimpanan penuh\".",
                "Komponen mana yang menyimpan foto secara permanen?",
                "Foto disimpan di storage agar tetap ada setelah ponsel dimatikan. Menghapus file yang tidak perlu atau " +
                "memindahkannya ke cloud akan mengosongkan storage.",
                "File apa yang biasanya memenuhi penyimpanan perangkatmu, dan bagaimana kamu mengelolanya?");
            var s4 = Sc(b, "sc_cashier", "Kasir Minimarket", "Minimarket dekat sekolah", "printer", keyboard, qs4,
                "Di minimarket, kasir mengetik jumlah barang, total belanja tampil di layar, lalu struk dicetak untuk pembeli.",
                "Perangkat mana yang memasukkan data, dan perangkat mana yang mengeluarkan hasil?",
                "Keyboard dan pemindai barcode adalah perangkat input. Monitor, printer struk, dan speaker adalah perangkat output. " +
                "CPU memproses total belanja di antaranya.",
                "Sebutkan satu tempat lain di sekitarmu yang memakai perangkat input dan output. Apa saja perangkatnya?");
            var s5 = Sc(b, "sc_typing_flow", "Dari Ketikan ke Layar", "Mengerjakan tugas di komputer", "workflow", vonNeumann, qs5,
                "Budi mengetik tugas bahasa Indonesia. Setiap huruf yang ditekan langsung muncul di layar dalam sekejap.",
                "Melewati komponen apa saja sebuah huruf sebelum muncul di layar?",
                "Huruf dari keyboard (input) disimpan di memori, diambil dan diproses CPU, lalu hasilnya dikirim ke monitor (output). " +
                "Semua melewati bus sistem sesuai arsitektur Von Neumann.",
                "Jelaskan dengan kata-katamu sendiri alur data ketika kamu mengirim pesan di ponsel.");

            // ------------------------------------------------------------------ Modules
            var m1 = Mod(b, "m1_cpu_vn", 1, "Arsitektur Von Neumann & CPU", "cpu", 25,
                "Kenali model Von Neumann dan cara CPU menjalankan instruksi melalui AR.",
                "Bagaimana pemahamanmu tentang tugas CPU dan hubungannya dengan memori setelah pelajaran ini?",
                new[]
                {
                    "Menjelaskan komponen utama arsitektur Von Neumann.",
                    "Membedakan fungsi Control Unit, ALU, dan register di dalam CPU.",
                    "Menghubungkan kerja CPU dengan situasi nyata sehari-hari."
                },
                new[] { vonNeumann, cpu }, new[] { s1 }, new[] { q1, q2, q3, q4 },
                Step(LessonStepType.VonNeumann), Step(LessonStepType.AR, cpu), Step(LessonStepType.Scenario, null, s1),
                Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            var m2 = Mod(b, "m2_memory_storage", 2, "Memori & Penyimpanan", "memory-stick", 20,
                "Bandingkan RAM dan storage, serta pahami mengapa data bisa hilang.",
                "Apa perbedaan RAM dan storage menurut kata-katamu sendiri?",
                new[]
                {
                    "Membedakan memori volatile (RAM) dan non-volatile (storage).",
                    "Menjelaskan peran RAM saat program berjalan.",
                    "Menerapkan cara mengelola memori dan penyimpanan perangkat."
                },
                new[] { memory, storage }, new[] { s2, s3 }, new[] { q5, q6, q7 },
                Step(LessonStepType.AR, memory), Step(LessonStepType.Scenario, null, s2), Step(LessonStepType.AR, storage),
                Step(LessonStepType.Scenario, null, s3), Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            var m3 = Mod(b, "m3_input_output", 3, "Perangkat Input & Output", "keyboard", 20,
                "Jelajahi perangkat yang memasukkan data dan yang menampilkan hasil.",
                "Perangkat input dan output apa yang paling sering kamu gunakan, dan untuk apa?",
                new[]
                {
                    "Mengelompokkan perangkat ke dalam input dan output.",
                    "Menjelaskan fungsi keyboard, mouse, monitor, printer, dan speaker.",
                    "Mengidentifikasi perangkat I/O dalam situasi nyata."
                },
                new[] { keyboard, mouse, monitor, printer, speaker }, new[] { s4 }, new[] { q8, q9, q10 },
                Step(LessonStepType.HardwareExplorer), Step(LessonStepType.AR, keyboard), Step(LessonStepType.Scenario, null, s4),
                Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            var m4 = Mod(b, "m4_data_flow", 4, "Alur Data Antar Komponen", "workflow", 20,
                "Ikuti perjalanan data dari input, memori, CPU, hingga output.",
                "Setelah melihat alur data, bagian mana yang paling mudah dan paling sulit kamu pahami?",
                new[]
                {
                    "Menjelaskan alur data input › memori › CPU › output.",
                    "Menjelaskan peran bus sistem.",
                    "Menerapkan konsep alur data pada contoh nyata."
                },
                new[] { vonNeumann }, new[] { s5 }, new[] { q11, q12 },
                Step(LessonStepType.VonNeumann), Step(LessonStepType.AR, vonNeumann), Step(LessonStepType.Scenario, null, s5),
                Step(LessonStepType.Practice), Step(LessonStepType.Reflection));

            foreach (var m in new[] { m1, m2, m3, m4 })
                foreach (var q in m.questions) q.relatedModuleId = m.moduleId;

            return b;
        }

        // ------------------------------------------------------------------ builders
        private static HardwareData Hw(Bundle b, string id, string name, string fullName, HardwareCategory cat, string icon,
            string target, string shortDesc, string detail, string function, string context, params HotspotData[] hotspots)
        {
            var h = ScriptableObject.CreateInstance<HardwareData>();
            h.name = "Hardware_" + id;
            h.hardwareId = id;
            h.hardwareName = name;
            h.fullName = fullName;
            h.category = cat;
            h.iconName = icon;
            h.arTargetName = target;
            h.shortDescription = shortDesc;
            h.detailedDescription = detail;
            h.function = function;
            h.contextualExample = context;
            h.hotspots = new List<HotspotData>(hotspots);
            h.printedWidthMeters = AppConstants.TargetPrintedWidthMeters;
            b.Hardware.Add(h);
            return h;
        }

        private static HotspotData Hs(string id, string label, string description, Vector3 pos) =>
            new HotspotData { hotspotId = id, label = label, description = description, localPosition = pos };

        private static QuestionData Q(Bundle b, string id, QuestionType type, HardwareData hw, string text, string correct,
            string explanation, params string[] options)
        {
            var q = ScriptableObject.CreateInstance<QuestionData>();
            q.name = "Question_" + id;
            q.questionId = id;
            q.questionType = type;
            q.relatedHardware = hw;
            q.iconName = hw != null ? hw.iconName : null;
            q.questionText = text;
            q.correctAnswer = correct;
            q.explanation = explanation;
            q.options = new List<string>(options);
            b.Questions.Add(q);
            return q;
        }

        private static ScenarioData Sc(Bundle b, string id, string title, string env, string icon, HardwareData hw,
            QuestionData task, string description, string question, string feedback, string reflection)
        {
            var s = ScriptableObject.CreateInstance<ScenarioData>();
            s.name = "Scenario_" + id;
            s.scenarioId = id;
            s.title = title;
            s.environment = env;
            s.iconName = icon;
            s.hardware = hw;
            s.ARTarget = hw.arTargetName;
            s.activity = task;
            s.description = description;
            s.question = question;
            s.feedback = feedback;
            s.reflection = reflection;
            b.Scenarios.Add(s);
            return s;
        }

        private static LessonStep Step(LessonStepType type, HardwareData hw = null, ScenarioData sc = null) =>
            new LessonStep { type = type, hardware = hw, scenario = sc };

        private static ModuleData Mod(Bundle b, string id, int order, string title, string icon, int minutes, string desc,
            string reflectionPrompt, string[] objectives, HardwareData[] hw, ScenarioData[] scenarios, QuestionData[] questions,
            params LessonStep[] steps)
        {
            var m = ScriptableObject.CreateInstance<ModuleData>();
            m.name = "Module_" + id;
            m.moduleId = id;
            m.order = order;
            m.title = title;
            m.iconName = icon;
            m.estimatedMinutes = minutes;
            m.description = desc;
            m.reflectionPrompt = reflectionPrompt;
            m.learningObjective = new List<string>(objectives);
            m.hardware = new List<HardwareData>(hw);
            m.scenarios = new List<ScenarioData>(scenarios);
            m.questions = new List<QuestionData>(questions);
            m.lessonSteps = new List<LessonStep>(steps);
            b.Modules.Add(m);
            return m;
        }
    }
}
