<template>
  <div class="container">
    <div class="container">
      <h3>Вибір алгоритму скремблювання</h3>
      <div class="algorithm-buttons">
        <button v-for="algorithm in algorithms"
                :key="algorithm.name"
                :style="{ backgroundColor: algorithm.color }"
                @click="addAlgorithm(algorithm)">
          {{ algorithm.label }}
        </button>
      </div>
      <div class="selected-algorithms">
        <p>Обрані алгоритми:</p>
        <div class="horizontal-list">
          <div v-for="(algorithm, index) in selectedAlgorithms"
               :key="index"
               class="algorithm-item"
               draggable="true"
               :class="{ dragging: dragIndex === index }"
               @dragstart="dragStart(index)"
               @dragover.prevent
               @drop="drop(index)"
               :style="{ backgroundColor: algorithm.color }">
            {{ algorithm.label }}
            <button @click="removeAlgorithm(index)">X</button>
          </div>
        </div>
      </div>

    </div>
    <div class="row">
      <input type="text" v-model="key" placeholder="Введіть ключ">
      <button @click="generateButton">Згенерувати</button>
    </div>
    <div class="row">
      
      <button @click="triggerFileInput">Обрати файл</button>
      <input id="fileInput" type="file" @change="handleFileChange" hidden>
      <span v-if="selectedFile">{{ selectedFile.name }}</span>
      <button @click="scrambleButton">Заскремблювати</button>
      <button @click="unscrambleButton">Розскремблювати</button>
    </div>


  </div>
</template>

<script>
  import axios from 'axios';

  export default {
    name: "app",
    components: {},
    data() {
      return {
        algorithms: [
          { name: "0", label: "XOR", color: "#ff9999" },
          { name: "1", label: "Перестановка блоків", color: "#99ccff" },
          { name: "2", label: "Цезар", color: "#99ff99" },
        ],
        selectedFile: null,
        selectedAlgorithms: [],
        dragIndex: null,
        outputData: "",
        key: ""
      };
    },
    methods: {
      handleFileChange(event) {
        this.selectedFile = event.target.files[0];
      },
      triggerFileInput() {
        document.getElementById("fileInput").click();
      },
      async generateButton() {
        try {
          const response = await axios.get("https://localhost:7168/api/Home/generateKey?length=32");
          this.key = response.data;
          console.log('Generated key:', this.key)
        } catch (error) {
          console.error('Error generating key:', error);
        }
      },
      async scrambleButton() {
        if (!this.selectedFile || !this.key || this.selectedAlgorithms.length === 0) {
          alert("Будь ласка, виберіть файл, введіть ключ і оберіть хоча б один алгоритм.");
          return;
        }
        const algorithmsToSend = this.selectedAlgorithms.map(algo => parseInt(algo.name));

        const formData = new FormData();
        formData.append("file", this.selectedFile);
        formData.append("key", this.key);
        formData.append("algorithms", JSON.stringify(algorithmsToSend));

        try {
          const response = await axios.post("https://localhost:7168/api/Home/scrambleFile", formData, {
            headers: { "Content-Type": "multipart/form-data" },
            responseType: "blob",
          }
          );

          const blob = new Blob([response.data], { type: "application/octet-stream" });
          const link = document.createElement("a");
          link.href = URL.createObjectURL(blob);
          link.download = "scrambled_" + this.selectedFile.name;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        } catch (error) {
          console.error("Помилка при обробці файлу:", error);
        }
      },
      async unscrambleButton() {
        if (!this.selectedFile || !this.key || this.selectedAlgorithms.length === 0) {
          alert("Будь ласка, виберіть файл, введіть ключ і оберіть хоча б один алгоритм.");
          return;
        }
        const formData = new FormData();
        const algorithmsToSend = this.selectedAlgorithms.map(algo => parseInt(algo.name));

        formData.append("file", this.selectedFile);
        formData.append("key", this.key);
        formData.append("algorithms", JSON.stringify(algorithmsToSend));

        try {
          const response = await axios.post("https://localhost:7168/api/Home/unscrambleFile", formData, {
            responseType: "blob"

          });

          let fileName = "unscrambled_file.dat";
          const contentDisposition = response.headers["content-disposition"];
          if (contentDisposition) {
            let fileNameMatch = contentDisposition.split("filename=")[1];
            if (fileNameMatch) {
              fileName = fileNameMatch.split(";")[0].trim().replace(/['"]/g, "");
            }
          }

          const url = window.URL.createObjectURL(new Blob([response.data]));
          const link = document.createElement("a");
          link.href = url;
          link.setAttribute("download", fileName);
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        } catch (error) {
          console.error("Помилка:", error.response?.data || error.message);
        }
      },

      addAlgorithm(algorithm) {
        this.selectedAlgorithms.push({
          ...algorithm,
          color: this.darkenColor(algorithm.color, 20),
        });
      },
      removeAlgorithm(index) {
        this.selectedAlgorithms.splice(index, 1);
      },
      dragStart(index) {
        this.dragIndex = index;
      },
      drop(index) {
        const draggedItem = this.selectedAlgorithms[this.dragIndex];
        this.selectedAlgorithms.splice(this.dragIndex, 1);
        this.selectedAlgorithms.splice(index, 0, draggedItem);
        this.dragIndex = null;
      },
      darkenColor(color, percent) {
        const num = parseInt(color.slice(1), 16);
        const amt = Math.round(2.55 * percent);
        const R = (num >> 16) - amt;
        const G = ((num >> 8) & 0x00ff) - amt;
        const B = (num & 0x0000ff) - amt;
        return `#${(0x1000000 + (R < 0 ? 0 : R > 255 ? 255 : R) * 0x10000 + (G < 0 ? 0 : G > 255 ? 255 : G) * 0x100 + (B < 0 ? 0 : B > 255 ? 255 : B))
          .toString(16)
          .slice(1)}`;
      },
    },
  };
</script>

<style>
  .row span {
    padding: 8px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    width: auto;
    border: 1px solid #ccc;
    border-radius: 8px;
  }
</style>
