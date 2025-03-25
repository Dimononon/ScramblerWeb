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
      <label for="key">Ключ</label>
      <input type="text" v-model="key" placeholder="Введіть ключ">
      <button @click="generateButton">Згенерувати</button>
    </div>
    <div class="file-upload-container">
      
    </div>
    <div class="buttons">
      <label class="file-label" for="fileInput">Обрати файл</label>
      <input id="fileInput" type="file" @change="handleFileChange" hidden>
      <span v-if="selectedFile" class="file-name">{{ selectedFile.name }}</span>
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
  .textarea-container {
    display: flex;
    justify-content: space-between;
  }

  textarea {
    width: 45%;
    height: 200px;
    resize: none;
    padding: 10px;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .container {
    margin: auto;
    background: #fff;
    padding: 20px;
    border: 1px solid #ccc;
    border-radius: 8px;
  }

    .container h3 {
      margin: 0 0 15px 0;
    }

  .row {
    display: flex;
    align-items: center;
    margin-bottom: 20px;
    margin-top: 20px;
    gap: 5px;
  }

    .row label {
      flex-shrink: 0;
      white-space: nowrap;
    }

    .row input[type="text"] {
      flex: 1;
      padding: 6px;
    }

    .row button {
      padding: 8px 15px;
      border: 1px solid #ccc;
      border-radius: 8px;
      cursor: pointer;
      flex-shrink: 0;
    }

  .buttons {
    display: flex;
    flex-direction: row;
    justify-content: left;
    align-items: center;
  }

    .buttons button {
      margin: 5px 10px;
      padding: 10px 10px 10px 10px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      width: 130px;
      border: 1px solid #ccc;
      border-radius: 8px;
    }
    

  .algorithm-buttons {
    display: flex;
    flex-direction: row;
    align-items: flex-start;
    gap: 10px;
    margin-bottom: 15px;
  }

    .algorithm-buttons button {
      padding: 10px 10px;
      color: black;
      border: 1px solid black;
      border-radius: 4px;
      cursor: pointer;
    }

      .algorithm-buttons button:hover {
        filter: brightness(85%);
      }

  .selected-algorithms {
    margin-top: 15px;
  }

  .horizontal-list {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }

  .algorithm-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px;
    border-radius: 4px;
    cursor: grab;
    min-width: 120px;
    color: black;
    border: 1px solid black
  }

    .algorithm-item.dragging {
      opacity: 0.7;
    }

    .algorithm-item button {
      background: white;
      color: black;
      border: none;
      border-radius: 50%;
      width: 20px;
      height: 20px;
      cursor: pointer;
      margin-left: 5px;
    }

      .algorithm-item button:hover {
        background: #ccc;
      }

  .file-upload-container {
    margin-top: 10px;
    display: flex;
    align-items: center;
  }

  .file-label {
    margin: 5px 0px;
    padding: 10px 10px 10px 10px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    width: 120px;
    border: 1px solid #ccc;
    border-radius: 8px;
  }

  .file-name {
    margin-left: 10px;
  }
</style>
