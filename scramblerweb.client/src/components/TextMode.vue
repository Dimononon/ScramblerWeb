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
      <input type="text" v-model="byteForm.key" placeholder="Введіть ключ">
      <button @click="generateButton">Згенерувати</button>
    </div>
    <div class="textarea-container">
      <textarea v-model="byteForm.data" placeholder="Вхідні дані (текст)"></textarea>
      <div class="buttons">
        <button @click="scrambleButton">Заскремблювати</button>
        <button @click="unscrambleButton">Розскремблювати</button>
      </div>
      <textarea v-model="outputData" placeholder="Вихідні дані"></textarea>
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
        selectedAlgorithms: [],
        dragIndex: null,
        outputData: "",
        byteForm: {
          key: "",
          data: "",
          algorithms: []
        }
      };
    },
    methods: {
      hexToBytes(hex) {
        const bytes = [];
        const sanitizedHex = hex.replace(/\s/g, '');
        for (let i = 0; i < sanitizedHex.length; i += 2) {
          bytes.push(parseInt(sanitizedHex.substring(i, i + 2), 16));
        }
        return bytes;
      },
      textToBytes(text) {
        const encoder = new TextEncoder();
        return Array.from(encoder.encode(text));
      },
      bytesToText(base64Data) {
        try {
          const byteString = atob(base64Data);
          const bytes = Array.from(byteString, (char) => char.charCodeAt(0));
          const decoder = new TextDecoder();
          return decoder.decode(new Uint8Array(bytes));
        } catch (error) {
          console.log("Can't convert base64 to text: " + error);
          return base64Data;
        }
      },
      bytesToHex(base64Data) {
        try {
          const byteString = atob(base64Data);
          const bytes = Array.from(byteString, (char) => char.charCodeAt(0));
          let hex = '';
          for (let byte of bytes) {
            hex += ('0' + (byte & 0xFF).toString(16)).slice(-2) + ' ';
          }
          return hex.trim();
        } catch (error) {
          console.log("Can't convert base64 to hex: " + error);
          return base64Data;
        }
      },
      addSpacesToHex(hexString) {
        if (!hexString) return "";
        let result = "";
        for (let i = 0; i < hexString.length; i += 2) {
          result += hexString.substring(i, Math.min(i + 2, hexString.length));
          if (i + 2 < hexString.length) {
            result += " ";
          }
        }

        return result.trim();
      },
      async generateButton() {
        try {
          const response = await axios.get("https://localhost:7168/api/Home/generateKey?length=32");
          this.byteForm.key = response.data;
          console.log('Generated key:', this.byteForm.key)
        } catch (error) {
          console.error('Error generating key:', error);
        }
      },
      async scrambleButton() {
        try {
          const algorithmsToSend = this.selectedAlgorithms.map(algo => parseInt(algo.name));
          const formData = {
            key: this.byteForm.key,
            data: this.textToBytes(this.byteForm.data),
            algorithms: algorithmsToSend
          };
          console.log(formData);
          const response = await axios.post("https://localhost:7168/api/Home/scramble", formData, {
            headers: {
              "Content-Type": "application/json"
            }
          });

          console.log("Response:", this.addSpacesToHex(response.data));
          this.outputData = this.addSpacesToHex(response.data);
        } catch (error) {
          console.error("Error submitting form:", error);
        }
      },
      async unscrambleButton() {
        try {
          const algorithmsToSend = this.selectedAlgorithms.map(algo => parseInt(algo.name));

          const formData = {
            key: this.byteForm.key,
            data: this.hexToBytes(this.byteForm.data),
            algorithms: algorithmsToSend
          };
          console.log(formData);
          const response = await axios.post("https://localhost:7168/api/Home/unscramble", formData, {
            headers: {
              "Content-Type": "application/json"
            }
          });

          console.log("Response:", response.data);
          this.outputData = this.bytesToText(response.data);
        } catch (error) {
          console.error("Error submitting form:", error);
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
    flex-direction: column;
    justify-content: center;
    align-items: center;
  }

    .buttons button {
      margin: 5px 10px;
      padding: 10px 10px 10px 10px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      width: 120px;
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
</style>
