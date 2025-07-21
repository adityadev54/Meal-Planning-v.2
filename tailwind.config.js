/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.cshtml',
    './Areas/**/*.cshtml',
    './Pages/**/*.cshtml'
  ],
  theme: {
    extend: {
      colors: {
        primary: '#14532d',
        accent: '#22c55e',
        gold: {
          light: '#FFF9E5',
          DEFAULT: '#FFD700',
          dark: '#BFA100'
        }
      },
      fontFamily: {
        sans: ['Inter', 'Arial', 'sans-serif'],
      },
      borderRadius: {
        'xl': '1.25rem',
      },
      boxShadow: {
        none: 'none',
      }
    }
  },
  plugins: [],
}

