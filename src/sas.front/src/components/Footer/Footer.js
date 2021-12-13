const Footer = () => {
    return (
        <section class="footer-area footer-dark">
            <div class="container">
                <div class="row justify-content-center">
                    <div class="col-lg-6">
                        <div class="footer-logo text-center">
                            <a class="mt-30" href="index.html">
                                <img src="https://sasfront.blob.core.windows.net/public/duke-logo-white.svg.png" width="150" alt="60" id="logoFooter" />
                            </a>
                        </div>
                        <div class="footer-support text-center">
                            <span class="number">+8801234567890</span>
                            <span class="mail">support@uideck.com</span>
                        </div>
                        <div class="copyright text-center mt-35">
                            <p class="text">&copy; <span id="year">{ new Date().getFullYear() }</span> - All rights reserved</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    )
}

export default Footer
