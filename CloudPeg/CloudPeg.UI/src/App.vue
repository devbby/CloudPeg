<script setup lang="ts">
const request = "/fs";

const handleSelectButton = {
  // show select button
  active: true,
  // allow multiple selection
  multiple: true,
  // handle click event
  click: (items:any, event:any) => {
    if (!items.length) {
      alert('No item selected');
      return;
    }
    alert('Selected: ' + items[0].path);
    console.log(items, event);
  },
};

</script>

<template>
  <h2>Hello World</h2>
  <div id="fs-container">
    <vue-finder
        id='my_vuefinder'  :url="request" :request="request" theme="light" :select-button="handleSelectButton"

    />
  </div>
  
</template>

<style scoped> 

#fs-container {   
}
</style>
