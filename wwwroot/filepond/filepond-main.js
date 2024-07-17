FilePond.registerPlugin(
    FilePondPluginImagePreview,
    FilePondPluginImageExifOrientation,
    FilePondPluginFileValidateSize,
);

function createFilePond(dotnetHelper, id, initialFile){
    var pond = FilePond.create(
        document.querySelector(id)
    );
    
    let initialFiles = [initialFile].filter(i=>i);
    
    // Configure FilePond settings
    pond.setOptions({
        server: {
            process: {
                url: '/poll/upload',
                method: 'POST',
                headers: {
                },
                onload: (response) => {
                    console.log("Uploaded file:", response);
                    return response;
                },
                onerror: (error) => console.error('Error uploading:', error)
            },
            load: (source, load, error, progress, abort, headers) => {
                console.log("Load", source, load, error);
                
                fetch("/file/get/"+ source)
                .then(response => {
                    if (!response.ok) {
                        error('Error loading the file');
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.blob(); // Convert the response to a blob
                })
                .then(blob=>{
                    load(blob);
                })
                
                return {
                    abort: () => {
                        abort();
                    },
                };
            },
            revert: null,
        },
        files: 
                initialFiles.map(file=>({
                    // the server file reference
                    source: file,
        
                    // set type to limbo to tell FilePond this is a temp file
                    options: {
                        type: 'local',
                    },
                })),
    });
    pond.on('processfile', (error, file) => {
        if (error) {
            console.error('Error during processing:', error);
        } else {
            console.log('File processed:', file);
            dotnetHelper.invokeMethodAsync('OnFileUploaded', file.serverId);
        }
    });
    pond.on('removefile', (error, file) => {
        console.log('File removed:', error, file);
        dotnetHelper.invokeMethodAsync('OnFileRemoved', file.serverId);
    });

    return pond;
}