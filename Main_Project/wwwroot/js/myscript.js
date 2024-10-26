function sendMail(){
    let parms ={
        email: document.getElementById("email").value,
        subject: document.getElementById("subject").value,
    }

    emailjs.send("service_42ir2g3","template_ux0oksa",parms).then(alert("Feed Back Send!!"))
}