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
  import api from '../api';

  export default {
    name: "app",
    components: {},
    data() {
      return {
        algorithms: [
          { name: "0", label: "XOR", color: "#ff9999" },
          { name: "1", label: "Перестановка блоків", color: "#99ccff" },
          { name: "2", label: "Адитивне перетворення", color: "#99ff99" },
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
          const response = await api.get("/api/Home/generateKey?length=32");
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
          const response = await api.post("/api/Home/scramble", formData);

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
          const response = await api.post("/api/Home/unscramble", formData);

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


