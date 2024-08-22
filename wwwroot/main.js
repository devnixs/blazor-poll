function initOptimisticRendering(selector){
let elements = document.querySelectorAll(selector);
elements.forEach(element=> element.onclick = (event)=> {
    if(!document.querySelector('.question.selected')){
       element.classList.add('selected')
    }
});
}

function scrollItemIntoView(selector){
    setTimeout(()=>{
    let element = document.querySelector(selector);
        if(element){
            console.log('Scrolling toward element', element);
            element.scrollIntoView();
        }
    }, 500);
}