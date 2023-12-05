<template>
  <div id="app">
    <h1>PDF Conversion App</h1>
    <div>
      <input type="file" @change="handleFileChange" />
      <button @click="handleSubmit" :disabled="!file">Отправить</button>
    </div>
    <table>
      <thead>
        <tr>
          <th>Имя файла</th>
          <th>ConversionId</th>
          <th>Статус</th>
          <th>Действия</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="file in files" :key="file.conversionId">
          <td>{{ file.fileName }}</td>
          <td>{{ file.conversionId }}</td>
          <td>{{ file.status }}</td>
          <td>
            <button @click="handleDownload(file.conversionId)" :disabled="file.status === 'в обработке'">Скачать</button>
            <button @click="handleDelete(file.conversionId)" :disabled="file.status === 'в обработке'">Удалить</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  data() {
    return {
      file: null,
      fileName: '',
      conversionId: '',
      files: [],
    };
  },
  methods: {
    async handleSubmit() {
      try {
        const formData = new FormData();
        formData.append('File', this.file);

        const response = await axios.post('http://localhost:5007/api/pdf/ConvertHtmlToPdf', formData);
        const newFile = {
          fileName: this.fileName,
          conversionId: response.data.conversionId,
          status: 'в обработке',
        };

        this.files = [...this.files, newFile];

        this.file = null;
        this.fileName = '';
      } catch (error) {
        console.error('Error submitting file:', error);
      }
    },

  async handleDownload(conversionId) {
  try {
    const response = await axios.get(`http://localhost:5007/api/pdf/DownloadResult/${conversionId}`, {
      responseType: 'blob', 
    });

    const link = document.createElement('a');
    const blob = new Blob([response.data], { type: 'application/octet-stream' });
    const blobURL = window.URL.createObjectURL(blob);

    link.href = blobURL;
    link.download = `result-${conversionId}.pdf`;
    link.target = '_blank';

    document.body.appendChild(link);

    link.click();

    document.body.removeChild(link);

    this.files = this.files.map((file) =>
      file.conversionId === conversionId
        ? { ...file, status: 'загружен' }
        : file
    );
  } catch (error) {
    console.error('Error downloading file:', error);
  }
},

    handleDelete(conversionId) {
      this.files = this.files.filter((file) => file.conversionId !== conversionId);
    },

    async handleFileChange(event) {
      const selectedFile = event.target.files[0];
      this.file = selectedFile;
      this.fileName = selectedFile.name;
    },

    startTimer() {
      this.timerId = setInterval(async () => {
        await this.checkFileStatuses();
      }, 5000);
    },

    stopTimer() {
      clearInterval(this.timerId);
    },

    async checkFileStatuses() {
      try {
        const filesInProgress = this.files.filter((file) => file.status === 'в обработке');

        for (const fileInProgress of filesInProgress) {
          const response = await axios.get(`http://localhost:5007/api/pdf/CheckStatus/${fileInProgress.conversionId}`);
          if (response.status === 200 && response.data === 'Found') {
            this.files = this.files.map((file) =>
              file.conversionId === fileInProgress.conversionId
                ? { ...file, status: 'обработан' }
                : file
            );
          }
        }
      } catch (error) {
        console.error('Error checking file statuses:', error);
      }
    },
  },
  mounted() {
    this.startTimer();
  },
  beforeDestroy() {
    this.stopTimer();
  },
};
</script>

<style>
/* Ваши стили могут быть добавлены здесь */
</style>