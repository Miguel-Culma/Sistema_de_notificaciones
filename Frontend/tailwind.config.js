import daisyui from 'daisyui'

/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['"IBM Plex Sans"', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [daisyui],
  daisyui: {
    themes: [
      {
        facturas: {
          primary: '#1d5f7a',
          'primary-content': '#ffffff',
          secondary: '#3b4a54',
          accent: '#b8700f',
          neutral: '#14262e',
          'neutral-content': '#e6eef1',
          'base-100': '#ffffff',
          'base-200': '#eef2f4',
          'base-300': '#d9e1e5',
          'base-content': '#14262e',
          info: '#2b7fb0',
          success: '#2f8f5b',
          warning: '#e0a526',
          'warning-content': '#2b1d00',
          error: '#c0432f',
        },
      },
    ],
  },
}
