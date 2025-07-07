const MainApp = Vue.createApp({

    inject: ['xx', 'yy'],


    async mounted() {
        console.log("mounted app")
       

    },

    async created(){

      

    },

    data() {
        return {
           
        }
    },

    watch: {

    },

    methods: {


        async post(workerKey, documentVersions){


            return await fetch("/Teams/AddBulkDocumentVersionSignoff/", {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(
                    {
                        workerKey: parseInt(workerKey),
                        documentVersions
                    }),
                signal: AbortSignal.timeout((1000*60)*10)

            }).then(async (response) => {
                if (response.ok) {
                    return response.json();
                }
                await response.text()
                    .then(message=>{
                        throw new Error(response.status + ": " + response.statusText + ": " + message.split(/\r?\n/)[0]);
                    });
            }).then(data=>{
                return data;
            })
                .catch((error)=>{
                    alert("Could not add document signoff: " +error.message);
                })


        },


        async get() {
            return fetch("/Data/GetEcwSkills/", {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
            }).then((response) => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error(response.status + ": " + response.statusText);
            })
                .then(value => {
                    return value;
                }).catch(error => {
                    alert(error);
                });
        },
    },


});