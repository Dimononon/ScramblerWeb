
<template>
  <div id="app">

    <div class="container" style="margin-bottom:5px;">
      <label>Оберіть режим роботи</label>
      <select class="custom-select" v-model="mode">
        <option value="0">Скремблювати байти</option>
        <option value="1">Скремблювати текст</option>
        <option value="2">Скремблювати файли</option>
      </select>
    </div>
    <div>
      <ByteMode v-show="mode==0"></ByteMode>
    </div>
  </div>

</template>

<script>
  import ByteMode from "./components/ByteMode.vue";

  export default {
    name: "app",
    data() {
      return {
        mode: 0
      };

    },
    components: {
      ByteMode

    }
  }
</script>
<style>
  .custom-select {
    width: 100%;
    padding: 10px;
    border: 1px solid #ccc;
    border-radius: 8px;
    background-color: #fff;
    font-size: 16px;
    color: #333;
    appearance: none;
    -moz-appearance: none;
    -webkit-appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='14' height='14' viewBox='0 0 24 24'%3E%3Cpath fill='%23333' d='M7 10l5 5 5-5z'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: calc(100% - 10px) center;
    background-size: 14px;
  }

    .custom-select:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 5px rgba(0, 123, 255, 0.5);
    }

    .custom-select option {
      font-size: 16px;
      padding: 10px;
    }

  .mode {
    border: 1px;
    color: red;
  }

  .container {
    margin: auto;
    background: #fff;
    padding: 20px;
    border: 1px solid #ccc;
    border-radius: 8px;
  }

  label {
    display: inline-block;
    margin-bottom: 5px;
    font-size: 16px;
    font-weight: 500;
    color: #333;
  }
</style>
