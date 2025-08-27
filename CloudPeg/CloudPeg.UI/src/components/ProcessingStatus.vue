<script lang="ts">
import {defineComponent} from "vue"; 

export default defineComponent({
  props: ['status'],
  
  setup()  {

  },

  components: {},

  // Enqueued = 1,
  // Processing = 2,
  // Completed = 3,
  // Failed = 4

  data():{ 
    statusText: string,
    isSecondary: boolean,
    isPrimary: boolean,
    isDanger: boolean,
    isSuccess: boolean,
  }{
    return { 
      statusText: '',
      isSecondary: false,
      isPrimary: false,
      isDanger: false,
      isSuccess: false,
    }
  },
  
  watch: {

    status() {
      this.setStatus(this.status)


    }
      
  },
  
  async mounted(){
    this.setStatus(this.status);
  },

  computed:{

  },

  methods:{
    setStatus(status: number){

      switch (this.status){

        case 1:
          this.statusText = "Enqueued"
          this.isSecondary = true;
          this.isDanger = false;
          this.isPrimary = false;
          this.isSuccess = false;
          break;
        case 2:
          this.statusText = "Processing"
          this.isSecondary = false;
          this.isDanger = false;
          this.isPrimary = true;
          this.isSuccess = false;
          break;
        case 3:
          this.statusText = "Completed"
          this.isSecondary = false;
          this.isDanger = false;
          this.isPrimary = false;
          this.isSuccess = true;
          break;
        case 4:
          this.statusText = "Failed"
          this.isSecondary = false;
          this.isDanger = true;
          this.isPrimary = false;
          this.isSuccess = false;
          break;
        default:
          this.statusText = "Unknown: " + this.status;
          this.isSecondary = false;
          this.isDanger = true;
          this.isPrimary = false;
          this.isSuccess = false;
      }
      
      
    }
  }
});
</script>

<template>

  <span :class="{
                  'badge': true,  
                  'rounded-pill': true, 
                  'text-bg-secondary': isSecondary, 
                  'text-bg-primary': isPrimary,  
                  'text-bg-danger': isDanger, 
                  'text-bg-success': isSuccess, 
                
                } ">{{statusText}}</span>
  
</template>

<style scoped>

</style>